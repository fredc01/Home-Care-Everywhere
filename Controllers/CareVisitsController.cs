using System.Security.Claims;
using HomeCarePH.Data;
using HomeCarePH.Domain.Entities;
using HomeCarePH.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers;

[Authorize]
public class CareVisitsController(ApplicationDbContext db) : Controller
{
    private const int PageSize = 15;

    public async Task<IActionResult> Index(bool history = false, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.CareVisits.AsNoTracking().Where(x => x.Resident.Household.Memberships.Any(m => m.UserId == userId));
        query = history ? query.Where(x => x.ScheduledAtUtc < DateTime.UtcNow || x.Status == CareVisitStatus.Completed || x.Status == CareVisitStatus.Cancelled) : query.Where(x => x.ScheduledAtUtc >= DateTime.UtcNow && x.Status != CareVisitStatus.Cancelled);
        var total = await query.CountAsync(cancellationToken);
        var visits = await query.OrderBy(x => x.ScheduledAtUtc).Skip((page - 1) * PageSize).Take(PageSize).Select(x => new CareVisitListItemViewModel { Id = x.Id, ResidentName = x.Resident.FullName, HouseholdName = x.Resident.Household.Name, CaregiverName = x.Caregiver.FullName, ScheduledAtUtc = x.ScheduledAtUtc, ExpectedDurationMinutes = x.ExpectedDurationMinutes, Status = x.Status }).ToListAsync(cancellationToken);
        return View(new CareVisitListViewModel { Visits = visits, History = history, Page = page, TotalCount = total });
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken) => View("Form", await Hydrate(new CareVisitFormViewModel(), cancellationToken));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CareVisitFormViewModel model, CancellationToken cancellationToken)
    {
        if (!await ResidentIsAuthorized(model.ResidentId, cancellationToken)) return Forbid();
        if (!await CaregiverIsAssigned(model.CaregiverId, model.ResidentId, cancellationToken)) ModelState.AddModelError(nameof(model.CaregiverId), "Select a caregiver with an active assignment to this resident or household.");
        var startUtc = ToUtc(model.ScheduledAtLocal);
        if (await HasConflict(model.CaregiverId, startUtc, model.ExpectedDurationMinutes, null, cancellationToken)) model.ConflictMessage = "This caregiver already has a non-cancelled visit during that time. Choose another time or caregiver.";
        if (!string.IsNullOrWhiteSpace(model.ConflictMessage)) ModelState.AddModelError(nameof(model.ScheduledAtLocal), model.ConflictMessage);
        model = await Hydrate(model, cancellationToken);
        if (!ModelState.IsValid) return View("Form", model);
        var visit = new CareVisit { ResidentId = model.ResidentId, CaregiverId = model.CaregiverId, ScheduledAtUtc = startUtc, ExpectedDurationMinutes = model.ExpectedDurationMinutes, Status = model.Status, Instructions = Clean(model.Instructions), Notes = Clean(model.Notes), CompletionNotes = Clean(model.CompletionNotes), UpdatedAtUtc = DateTime.UtcNow };
        ApplyStatusTimestamps(visit);
        db.CareVisits.Add(visit);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = visit.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var visit = await AuthorizedVisits().Include(x => x.Resident).ThenInclude(x => x.Household).Include(x => x.Caregiver).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (visit is null) return NotFound();
        return View(new CareVisitDetailsViewModel { Visit = visit, HasConflict = await HasConflict(visit.CaregiverId, visit.ScheduledAtUtc, visit.ExpectedDurationMinutes, visit.Id, cancellationToken) });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var visit = await AuthorizedVisits().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (visit is null) return NotFound();
        return View("Form", await Hydrate(new CareVisitFormViewModel { Id = id, ResidentId = visit.ResidentId, CaregiverId = visit.CaregiverId, ScheduledAtLocal = visit.ScheduledAtUtc.ToLocalTime(), ExpectedDurationMinutes = visit.ExpectedDurationMinutes, Status = visit.Status, Instructions = visit.Instructions, Notes = visit.Notes, CompletionNotes = visit.CompletionNotes }, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CareVisitFormViewModel model, CancellationToken cancellationToken)
    {
        var visit = await AuthorizedVisits().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (visit is null) return NotFound();
        if (!await ResidentIsAuthorized(model.ResidentId, cancellationToken)) return Forbid();
        if (!await CaregiverIsAssigned(model.CaregiverId, model.ResidentId, cancellationToken)) ModelState.AddModelError(nameof(model.CaregiverId), "Select a caregiver with an active assignment to this resident or household.");
        var startUtc = ToUtc(model.ScheduledAtLocal);
        if (await HasConflict(model.CaregiverId, startUtc, model.ExpectedDurationMinutes, id, cancellationToken)) model.ConflictMessage = "This caregiver already has a non-cancelled visit during that time. Choose another time or caregiver.";
        if (!string.IsNullOrWhiteSpace(model.ConflictMessage)) ModelState.AddModelError(nameof(model.ScheduledAtLocal), model.ConflictMessage);
        model = await Hydrate(model, cancellationToken);
        if (!ModelState.IsValid) return View("Form", model);
        visit.ResidentId = model.ResidentId; visit.CaregiverId = model.CaregiverId; visit.ScheduledAtUtc = startUtc; visit.ExpectedDurationMinutes = model.ExpectedDurationMinutes; visit.Status = model.Status; visit.Instructions = Clean(model.Instructions); visit.Notes = Clean(model.Notes); visit.CompletionNotes = Clean(model.CompletionNotes); visit.UpdatedAtUtc = DateTime.UtcNow;
        ApplyStatusTimestamps(visit);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<CareVisit> AuthorizedVisits() { var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!; return db.CareVisits.Where(x => x.Resident.Household.Memberships.Any(m => m.UserId == userId)); }
    private async Task<bool> ResidentIsAuthorized(int residentId, CancellationToken cancellationToken) => await db.Residents.AnyAsync(x => x.Id == residentId && x.IsActive && x.Household.Memberships.Any(m => m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)), cancellationToken);
    private async Task<bool> CaregiverIsAssigned(int caregiverId, int residentId, CancellationToken cancellationToken) => await db.CaregiverAssignments.AnyAsync(x => x.CaregiverId == caregiverId && x.IsActive && x.Resident != null && x.ResidentId == residentId || x.CaregiverId == caregiverId && x.IsActive && x.ResidentId == null && x.HouseholdId == db.Residents.Where(r => r.Id == residentId).Select(r => r.HouseholdId).FirstOrDefault(), cancellationToken);
    private async Task<bool> HasConflict(int caregiverId, DateTime startUtc, int durationMinutes, int? excludedVisitId, CancellationToken cancellationToken)
    {
        var endUtc = startUtc.AddMinutes(durationMinutes);
        var candidates = await db.CareVisits.Where(x => x.CaregiverId == caregiverId && x.Status != CareVisitStatus.Cancelled && (!excludedVisitId.HasValue || x.Id != excludedVisitId.Value)).Select(x => new { x.Id, x.ScheduledAtUtc, x.ExpectedDurationMinutes }).ToListAsync(cancellationToken);
        return candidates.Any(x => x.ScheduledAtUtc < endUtc && x.ScheduledAtUtc.AddMinutes(x.ExpectedDurationMinutes) > startUtc);
    }
    private async Task<CareVisitFormViewModel> Hydrate(CareVisitFormViewModel model, CancellationToken cancellationToken) => new() { Id = model.Id, ResidentId = model.ResidentId, CaregiverId = model.CaregiverId, ScheduledAtLocal = model.ScheduledAtLocal, ExpectedDurationMinutes = model.ExpectedDurationMinutes, Status = model.Status, Instructions = model.Instructions, Notes = model.Notes, CompletionNotes = model.CompletionNotes, ConflictMessage = model.ConflictMessage, Residents = await GetResidents(cancellationToken), Caregivers = await GetCaregivers(model.ResidentId, cancellationToken) };
    private async Task<List<CareVisitResidentOption>> GetResidents(CancellationToken cancellationToken) => await db.Residents.Where(x => x.IsActive && x.Household.Memberships.Any(m => m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))).OrderBy(x => x.FullName).Select(x => new CareVisitResidentOption(x.Id, x.FullName, x.Household.Name)).ToListAsync(cancellationToken);
    private async Task<List<CareVisitCaregiverOption>> GetCaregivers(int residentId, CancellationToken cancellationToken) => await db.CaregiverAssignments.Where(x => x.IsActive && x.Caregiver.IsActive && x.ResidentId == residentId || x.IsActive && x.Caregiver.IsActive && x.ResidentId == null && x.HouseholdId == db.Residents.Where(r => r.Id == residentId).Select(r => r.HouseholdId).FirstOrDefault()).Select(x => new CareVisitCaregiverOption(x.CaregiverId, x.Caregiver.FullName, x.Caregiver.Role)).Distinct().OrderBy(x => x.Name).ToListAsync(cancellationToken);
    private static DateTime ToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified));
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void ApplyStatusTimestamps(CareVisit visit) { if (visit.Status == CareVisitStatus.InProgress && visit.StartedAtUtc is null) visit.StartedAtUtc = DateTime.UtcNow; if (visit.Status == CareVisitStatus.Completed) { visit.StartedAtUtc ??= DateTime.UtcNow; visit.CompletedAtUtc ??= DateTime.UtcNow; } if (visit.Status != CareVisitStatus.Completed) visit.CompletedAtUtc = null; }
}
