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
public class MaintenanceController(ApplicationDbContext db, HouseholdAccessService access) : Controller
{
    public async Task<IActionResult> Index(int? propertyId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.MaintenanceTasks.Where(x => x.Property.Household.Memberships.Any(m => m.UserId == userId));
        if (propertyId.HasValue) query = query.Where(x => x.PropertyId == propertyId.Value);
        var tasks = await query.OrderBy(x => x.IsActive ? 0 : 1).ThenBy(x => x.NextDueDate).Select(x => new MaintenanceListItemViewModel { Id = x.Id, PropertyId = x.PropertyId, PropertyName = x.Property.Name, Title = x.Title, ApplianceName = x.Appliance == null ? null : x.Appliance.Name, Frequency = x.Frequency, NextDueDate = x.NextDueDate, IsActive = x.IsActive, CompletionCount = x.Completions.Count }).ToListAsync(cancellationToken);
        ViewData["PropertyId"] = propertyId;
        return View(tasks);
    }

    public async Task<IActionResult> Create(int propertyId, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(propertyId, cancellationToken);
        if (property is null) return Forbid();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        return View(await BuildCreateModel(propertyId, cancellationToken));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMaintenanceViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (model.Frequency == MaintenanceFrequency.Custom && (!model.CustomIntervalDays.HasValue || model.CustomIntervalDays.Value < 1)) ModelState.AddModelError(nameof(model.CustomIntervalDays), "Add the number of days between recurring tasks.");
        if (model.Frequency != MaintenanceFrequency.Custom) model.CustomIntervalDays = null;
        if (model.ApplianceId.HasValue && !await db.Appliances.AnyAsync(x => x.Id == model.ApplianceId && x.PropertyId == model.PropertyId, cancellationToken)) ModelState.AddModelError(nameof(model.ApplianceId), "Select an appliance from this property.");
        if (!ModelState.IsValid) { model.Appliances = await db.Appliances.Where(x => x.PropertyId == model.PropertyId).OrderBy(x => x.Name).ToListAsync(cancellationToken); return View(model); }
        var task = new MaintenanceTask { PropertyId = model.PropertyId, ApplianceId = model.ApplianceId, Title = model.Title.Trim(), Description = model.Description?.Trim(), Frequency = model.Frequency, CustomIntervalDays = model.CustomIntervalDays, NextDueDate = model.NextDueDate, CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)! };
        db.MaintenanceTasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var task = await db.MaintenanceTasks.Include(x => x.Property).ThenInclude(x => x.Household).Include(x => x.Appliance).Include(x => x.CreatedByUser).Include(x => x.Completions.OrderByDescending(c => c.CompletedAtUtc)).ThenInclude(x => x.CompletedByUser).Include(x => x.Completions).ThenInclude(x => x.Expenses).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (task is null) return NotFound();
        var role = await access.GetRoleAsync(User, task.Property.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        return View(new MaintenanceDetailsViewModel { Task = task, CurrentRole = role.Value, Completion = new CompleteMaintenanceViewModel { MaintenanceTaskId = id } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CompleteMaintenanceViewModel model, CancellationToken cancellationToken)
    {
        var task = await db.MaintenanceTasks.Include(x => x.Property).SingleOrDefaultAsync(x => x.Id == model.MaintenanceTaskId, cancellationToken);
        if (task is null) return NotFound();
        var role = await access.GetRoleAsync(User, task.Property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        var completion = new MaintenanceCompletion { MaintenanceTaskId = task.Id, Notes = model.Notes?.Trim(), CompletedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)! };
        db.MaintenanceCompletions.Add(completion);
        if (model.ExpenseAmount.HasValue && model.ExpenseAmount.Value > 0) completion.Expenses.Add(new MaintenanceExpense { Amount = model.ExpenseAmount.Value, Vendor = model.Vendor?.Trim(), Description = model.ExpenseDescription?.Trim() });
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (task.Frequency == MaintenanceFrequency.OneTime) task.IsActive = false;
        else task.NextDueDate = NextDueDate(task.NextDueDate < today ? today : task.NextDueDate, task.Frequency, task.CustomIntervalDays);
        task.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    private static DateOnly NextDueDate(DateOnly date, MaintenanceFrequency frequency, int? customDays) => frequency switch
    {
        MaintenanceFrequency.Weekly => date.AddDays(7),
        MaintenanceFrequency.Monthly => date.AddMonths(1),
        MaintenanceFrequency.Quarterly => date.AddMonths(3),
        MaintenanceFrequency.SemiAnnual => date.AddMonths(6),
        MaintenanceFrequency.Annual => date.AddYears(1),
        MaintenanceFrequency.Custom => date.AddDays(customDays ?? 1),
        _ => date
    };

    private async Task<CreateMaintenanceViewModel> BuildCreateModel(int propertyId, CancellationToken cancellationToken) => new() { PropertyId = propertyId, Appliances = await db.Appliances.Where(x => x.PropertyId == propertyId).OrderBy(x => x.Name).ToListAsync(cancellationToken) };

    private async Task<Property?> GetPropertyForUser(int propertyId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await db.Properties.AsNoTracking().SingleOrDefaultAsync(x => x.Id == propertyId && x.Household.Memberships.Any(m => m.UserId == userId), cancellationToken);
    }
}
