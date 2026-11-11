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
public class CaregiversController(ApplicationDbContext db, HouseholdAccessService access) : Controller
{
    private const int PageSize = 12;

    public async Task<IActionResult> Index(string? search, bool includeInactive = false, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.Caregivers.AsNoTracking().Where(x => x.Assignments.Any(a => a.Household.Memberships.Any(m => m.UserId == userId)) || db.CareVisits.Any(v => v.CaregiverId == x.Id && v.Resident.Household.Memberships.Any(m => m.UserId == userId)));
        if (!includeInactive) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.ILike(x.FullName, $"%{term}%") || (x.Role != null && EF.Functions.ILike(x.Role, $"%{term}%")) || (x.Specializations != null && EF.Functions.ILike(x.Specializations, $"%{term}%")));
        }
        var total = await query.CountAsync(cancellationToken);
        var caregivers = await query.OrderBy(x => x.FullName).Skip((page - 1) * PageSize).Take(PageSize).Select(x => new CaregiverListItemViewModel
        {
            Id = x.Id, FullName = x.FullName, Role = x.Role, ContactNumber = x.ContactNumber, IsActive = x.IsActive,
            AssignmentCount = x.Assignments.Count(a => a.IsActive && a.Household.Memberships.Any(m => m.UserId == userId)),
            VisitCount = db.CareVisits.Count(v => v.CaregiverId == x.Id && v.Resident.Household.Memberships.Any(m => m.UserId == userId)),
            NextVisitUtc = db.CareVisits.Where(v => v.CaregiverId == x.Id && v.Status != CareVisitStatus.Cancelled && v.ScheduledAtUtc >= DateTime.UtcNow && v.Resident.Household.Memberships.Any(m => m.UserId == userId)).OrderBy(v => v.ScheduledAtUtc).Select(v => (DateTime?)v.ScheduledAtUtc).FirstOrDefault()
        }).ToListAsync(cancellationToken);
        return View(new CaregiverListViewModel { Caregivers = caregivers, Search = search?.Trim() ?? string.Empty, IncludeInactive = includeInactive, Page = page, TotalCount = total });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var households = await GetHouseholds(cancellationToken);
        if (households.Count == 0) return Forbid();
        return View("Form", new CaregiverFormViewModel { HouseholdId = households[0].Id, Households = households, Residents = await GetResidents(households[0].Id, cancellationToken) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CaregiverFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessHousehold(model.HouseholdId, cancellationToken)) return Forbid();
        if (model.ResidentId.HasValue && !await ResidentBelongsToAuthorizedHousehold(model.ResidentId.Value, model.HouseholdId, cancellationToken)) return Forbid();
        model = await HydrateForm(model, cancellationToken);
        if (!ModelState.IsValid) return View("Form", model);
        var caregiver = new Caregiver { FullName = model.FullName.Trim(), Role = Clean(model.Role), Email = Clean(model.Email), ContactNumber = Clean(model.ContactNumber), Skills = Clean(model.Skills), Qualifications = Clean(model.Qualifications), Specializations = Clean(model.Specializations), Availability = Clean(model.Availability), Notes = Clean(model.Notes), IsActive = model.IsActive };
        db.Caregivers.Add(caregiver);
        await db.SaveChangesAsync(cancellationToken);
        db.CaregiverAssignments.Add(new CaregiverAssignment { CaregiverId = caregiver.Id, HouseholdId = model.HouseholdId, ResidentId = model.ResidentId, Notes = "Initial assignment" });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = caregiver.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        if (!await CanAccessCaregiver(id, cancellationToken)) return Forbid();
        var caregiver = await db.Caregivers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (caregiver is null) return NotFound();
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var assignments = await db.CaregiverAssignments.AsNoTracking().Include(x => x.Household).Include(x => x.Resident).Where(x => x.CaregiverId == id && x.Household.Memberships.Any(m => m.UserId == userId)).OrderByDescending(x => x.IsActive).ThenByDescending(x => x.AssignedAtUtc).ToListAsync(cancellationToken);
        var visits = db.CareVisits.AsNoTracking().Include(x => x.Resident).ThenInclude(x => x.Household).Where(x => x.CaregiverId == id && x.Resident.Household.Memberships.Any(m => m.UserId == userId));
        var upcoming = await visits.Where(x => x.ScheduledAtUtc >= DateTime.UtcNow && x.Status != CareVisitStatus.Cancelled).OrderBy(x => x.ScheduledAtUtc).Take(12).ToListAsync(cancellationToken);
        var previous = await visits.Where(x => x.ScheduledAtUtc < DateTime.UtcNow || x.Status == CareVisitStatus.Completed).OrderByDescending(x => x.ScheduledAtUtc).Take(12).ToListAsync(cancellationToken);
        var households = await GetHouseholds(cancellationToken);
        return View(new CaregiverDetailsViewModel { Caregiver = caregiver, Assignments = assignments, UpcomingVisits = upcoming, PreviousVisits = previous, Households = households, Residents = await GetResidents(null, cancellationToken) });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        if (!await CanAccessCaregiver(id, cancellationToken)) return Forbid();
        var caregiver = await db.Caregivers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (caregiver is null) return NotFound();
        var householdId = await db.CaregiverAssignments.Where(x => x.CaregiverId == id && x.IsActive && x.Household.Memberships.Any(m => m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))).Select(x => (int?)x.HouseholdId).FirstOrDefaultAsync(cancellationToken);
        var model = new CaregiverFormViewModel { Id = id, FullName = caregiver.FullName, Role = caregiver.Role, Email = caregiver.Email, ContactNumber = caregiver.ContactNumber, Skills = caregiver.Skills, Qualifications = caregiver.Qualifications, Specializations = caregiver.Specializations, Availability = caregiver.Availability, Notes = caregiver.Notes, IsActive = caregiver.IsActive, HouseholdId = householdId ?? 0 };
        return View("Form", await HydrateForm(model, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CaregiverFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessCaregiver(id, cancellationToken)) return Forbid();
        if (!await CanAccessHousehold(model.HouseholdId, cancellationToken)) return Forbid();
        if (model.ResidentId.HasValue && !await ResidentBelongsToAuthorizedHousehold(model.ResidentId.Value, model.HouseholdId, cancellationToken)) return Forbid();
        var caregiver = await db.Caregivers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (caregiver is null) return NotFound();
        model = await HydrateForm(model, cancellationToken);
        if (!ModelState.IsValid) return View("Form", model);
        caregiver.FullName = model.FullName.Trim(); caregiver.Role = Clean(model.Role); caregiver.Email = Clean(model.Email); caregiver.ContactNumber = Clean(model.ContactNumber); caregiver.Skills = Clean(model.Skills); caregiver.Qualifications = Clean(model.Qualifications); caregiver.Specializations = Clean(model.Specializations); caregiver.Availability = Clean(model.Availability); caregiver.Notes = Clean(model.Notes); caregiver.IsActive = model.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(CaregiverAssignmentFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await CanAccessCaregiver(model.CaregiverId, cancellationToken)) return Forbid();
        if (!await CanAccessHousehold(model.HouseholdId, cancellationToken)) return Forbid();
        if (model.ResidentId.HasValue && !await ResidentBelongsToAuthorizedHousehold(model.ResidentId.Value, model.HouseholdId, cancellationToken)) return Forbid();
        if (!await db.CaregiverAssignments.AnyAsync(x => x.CaregiverId == model.CaregiverId && x.HouseholdId == model.HouseholdId && x.ResidentId == model.ResidentId && x.IsActive, cancellationToken))
        {
            db.CaregiverAssignments.Add(new CaregiverAssignment { CaregiverId = model.CaregiverId, HouseholdId = model.HouseholdId, ResidentId = model.ResidentId, Notes = Clean(model.Notes) });
            await db.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Details), new { id = model.CaregiverId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EndAssignment(int id, CancellationToken cancellationToken)
    {
        var assignment = await db.CaregiverAssignments.Include(x => x.Caregiver).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (assignment is null) return NotFound();
        if (!await CanAccessHousehold(assignment.HouseholdId, cancellationToken)) return Forbid();
        assignment.IsActive = false; assignment.EndedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = assignment.CaregiverId });
    }

    private async Task<bool> CanAccessCaregiver(int id, CancellationToken cancellationToken) { var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!; return await db.Caregivers.AnyAsync(x => x.Id == id && (x.Assignments.Any(a => a.Household.Memberships.Any(m => m.UserId == userId)) || db.CareVisits.Any(v => v.CaregiverId == id && v.Resident.Household.Memberships.Any(m => m.UserId == userId))), cancellationToken); }
    private async Task<bool> CanAccessHousehold(int householdId, CancellationToken cancellationToken) => await access.GetRoleAsync(User, householdId, cancellationToken) is not null;
    private async Task<bool> ResidentBelongsToAuthorizedHousehold(int residentId, int householdId, CancellationToken cancellationToken) => await db.Residents.AnyAsync(x => x.Id == residentId && x.HouseholdId == householdId && x.Household.Memberships.Any(m => m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)), cancellationToken);
    private async Task<List<CareTeamHouseholdOption>> GetHouseholds(CancellationToken cancellationToken) => await db.HouseholdMemberships.Where(x => x.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).OrderBy(x => x.Household.Name).Select(x => new CareTeamHouseholdOption(x.HouseholdId, x.Household.Name)).ToListAsync(cancellationToken);
    private async Task<List<CareTeamResidentOption>> GetResidents(int? householdId, CancellationToken cancellationToken) => await db.Residents.Where(x => x.IsActive && (!householdId.HasValue || x.HouseholdId == householdId) && x.Household.Memberships.Any(m => m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))).OrderBy(x => x.FullName).Select(x => new CareTeamResidentOption(x.Id, x.HouseholdId, x.FullName)).ToListAsync(cancellationToken);
    private async Task<CaregiverFormViewModel> HydrateForm(CaregiverFormViewModel model, CancellationToken cancellationToken) => new() { Id = model.Id, FullName = model.FullName, Role = model.Role, Email = model.Email, ContactNumber = model.ContactNumber, Skills = model.Skills, Qualifications = model.Qualifications, Specializations = model.Specializations, Availability = model.Availability, Notes = model.Notes, IsActive = model.IsActive, HouseholdId = model.HouseholdId, ResidentId = model.ResidentId, Households = await GetHouseholds(cancellationToken), Residents = await GetResidents(model.HouseholdId == 0 ? null : model.HouseholdId, cancellationToken) };
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
