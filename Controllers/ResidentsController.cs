using System.Security.Claims;
using HomeCarePH.Data;
using HomeCarePH.Domain.Entities;
using HomeCarePH.Models;
using HomeCarePH.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers;

[Authorize]
public class ResidentsController(ApplicationDbContext db, HouseholdAccessService access) : Controller
{
    private const int DefaultPageSize = 12;

    public async Task<IActionResult> Index(string? search, int? householdId, bool includeArchived = false, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.Residents.AsNoTracking().Where(x => x.Household.Memberships.Any(m => m.UserId == userId));
        if (!includeArchived) query = query.Where(x => x.IsActive);
        if (householdId.HasValue) query = query.Where(x => x.HouseholdId == householdId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{term}%") || (x.ContactNumber != null && EF.Functions.ILike(x.ContactNumber, $"%{term}%")));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var residents = await query.OrderBy(x => x.FullName)
            .Skip((page - 1) * DefaultPageSize).Take(DefaultPageSize)
            .Select(x => new ResidentListItemViewModel
            {
                Id = x.Id, HouseholdId = x.HouseholdId, HouseholdName = x.Household.Name,
                FullName = x.FullName, ContactNumber = x.ContactNumber, DateOfBirth = x.DateOfBirth,
                IsActive = x.IsActive, VisitCount = 0,
                NextVisitUtc = db.CareVisits.Where(v => v.ResidentId == x.Id && v.Status != CareVisitStatus.Cancelled && v.ScheduledAtUtc >= DateTime.UtcNow).OrderBy(v => v.ScheduledAtUtc).Select(v => (DateTime?)v.ScheduledAtUtc).FirstOrDefault()
            }).ToListAsync(cancellationToken);

        // The visit count is intentionally calculated in a separate authorized query because CareVisit has no reverse navigation on Resident.
        var residentIds = residents.Select(x => x.Id).ToArray();
        var visitCounts = await db.CareVisits.Where(x => residentIds.Contains(x.ResidentId)).GroupBy(x => x.ResidentId).Select(x => new { Id = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Id, x => x.Count, cancellationToken);
        residents = residents.Select(x => new ResidentListItemViewModel { Id = x.Id, HouseholdId = x.HouseholdId, HouseholdName = x.HouseholdName, FullName = x.FullName, ContactNumber = x.ContactNumber, DateOfBirth = x.DateOfBirth, IsActive = x.IsActive, VisitCount = visitCounts.GetValueOrDefault(x.Id), NextVisitUtc = x.NextVisitUtc }).ToList();

        var households = await db.HouseholdMemberships.Where(x => x.UserId == userId).OrderBy(x => x.Household.Name).Select(x => new HouseholdFilterOption(x.HouseholdId, x.Household.Name)).ToListAsync(cancellationToken);
        return View(new ResidentListViewModel { Residents = residents, Households = households, Search = search?.Trim() ?? string.Empty, HouseholdId = householdId, IncludeArchived = includeArchived, Page = page, TotalCount = totalCount });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var households = await GetAuthorizedHouseholds(cancellationToken);
        if (households.Count == 0) return Forbid();
        return View(new ResidentFormViewModel { HouseholdId = households[0].Id, Households = households });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ResidentFormViewModel model, CancellationToken cancellationToken)
    {
        var role = await access.GetRoleAsync(User, model.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        model = await WithHouseholds(model, cancellationToken);
        if (!ModelState.IsValid) return View(model);
        var resident = new Resident { FullName = model.FullName.Trim(), ContactNumber = Clean(model.ContactNumber), DateOfBirth = model.DateOfBirth, HouseholdId = model.HouseholdId, EmergencyContactName = Clean(model.EmergencyContactName), EmergencyContactRelationship = Clean(model.EmergencyContactRelationship), EmergencyContactNumber = Clean(model.EmergencyContactNumber), CareRequirements = Clean(model.CareRequirements), CareNotes = Clean(model.CareNotes) };
        db.Residents.Add(resident);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = resident.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var resident = await db.Residents.Include(x => x.Household).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resident is null) return NotFound();
        var role = await access.GetRoleAsync(User, resident.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        var history = await db.CareVisits.AsNoTracking().Include(x => x.Caregiver).Where(x => x.ResidentId == id).OrderByDescending(x => x.ScheduledAtUtc).Take(20).ToListAsync(cancellationToken);
        return View(new ResidentDetailsViewModel { Resident = resident, CurrentRole = role.Value, CareHistory = history });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var resident = await GetAuthorizedResident(id, cancellationToken);
        if (resident is null) return Forbid();
        return View(await ToForm(resident, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ResidentFormViewModel model, CancellationToken cancellationToken)
    {
        var resident = await GetAuthorizedResident(id, cancellationToken);
        if (resident is null) return Forbid();
        var targetRole = await access.GetRoleAsync(User, model.HouseholdId, cancellationToken);
        if (targetRole is null || !HouseholdAccessService.CanEditProperty(targetRole.Value)) return Forbid();
        model = await WithHouseholds(model, cancellationToken);
        if (!ModelState.IsValid) return View(model);
        resident.FullName = model.FullName.Trim(); resident.ContactNumber = Clean(model.ContactNumber); resident.DateOfBirth = model.DateOfBirth; resident.HouseholdId = model.HouseholdId; resident.EmergencyContactName = Clean(model.EmergencyContactName); resident.EmergencyContactRelationship = Clean(model.EmergencyContactRelationship); resident.EmergencyContactNumber = Clean(model.EmergencyContactNumber); resident.CareRequirements = Clean(model.CareRequirements); resident.CareNotes = Clean(model.CareNotes); resident.IsActive = model.IsActive; resident.ArchivedAtUtc = model.IsActive ? null : resident.ArchivedAtUtc ?? DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        var resident = await GetAuthorizedResident(id, cancellationToken);
        if (resident is null) return Forbid();
        resident.IsActive = false; resident.ArchivedAtUtc ??= DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index), new { includeArchived = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var resident = await GetAuthorizedResident(id, cancellationToken);
        if (resident is null) return Forbid();
        resident.IsActive = true; resident.ArchivedAtUtc = null;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<Resident?> GetAuthorizedResident(int id, CancellationToken cancellationToken)
    {
        var resident = await db.Residents.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (resident is null) return null;
        var role = await access.GetRoleAsync(User, resident.HouseholdId, cancellationToken);
        return role is not null && HouseholdAccessService.CanEditProperty(role.Value) ? resident : null;
    }

    private async Task<List<HouseholdFilterOption>> GetAuthorizedHouseholds(CancellationToken cancellationToken) => await db.HouseholdMemberships.Where(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).OrderBy(x => x.Household.Name).Select(x => new HouseholdFilterOption(x.HouseholdId, x.Household.Name)).ToListAsync(cancellationToken);
    private async Task<ResidentFormViewModel> WithHouseholds(ResidentFormViewModel model, CancellationToken cancellationToken) => new() { Id = model.Id, FullName = model.FullName, ContactNumber = model.ContactNumber, DateOfBirth = model.DateOfBirth, HouseholdId = model.HouseholdId, EmergencyContactName = model.EmergencyContactName, EmergencyContactRelationship = model.EmergencyContactRelationship, EmergencyContactNumber = model.EmergencyContactNumber, CareRequirements = model.CareRequirements, CareNotes = model.CareNotes, IsActive = model.IsActive, Households = await GetAuthorizedHouseholds(cancellationToken) };
    private async Task<ResidentFormViewModel> ToForm(Resident resident, CancellationToken cancellationToken) => new() { Id = resident.Id, FullName = resident.FullName, ContactNumber = resident.ContactNumber, DateOfBirth = resident.DateOfBirth, HouseholdId = resident.HouseholdId, EmergencyContactName = resident.EmergencyContactName, EmergencyContactRelationship = resident.EmergencyContactRelationship, EmergencyContactNumber = resident.EmergencyContactNumber, CareRequirements = resident.CareRequirements, CareNotes = resident.CareNotes, IsActive = resident.IsActive, Households = await GetAuthorizedHouseholds(cancellationToken) };
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
