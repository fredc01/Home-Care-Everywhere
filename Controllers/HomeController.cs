using System.Diagnostics;
using HomeCarePH.Data;
using HomeCarePH.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCarePH.Controllers;

public class HomeController(ApplicationDbContext db, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysFromToday = today.AddDays(30);
        var warrantyCutoff = today.AddDays(60);
        var last30Days = DateTime.UtcNow.AddDays(-30);
        var model = new DashboardViewModel();

        try
        {
            var maintenance = await db.MaintenanceTasks.AsNoTracking()
                .Where(x => x.IsActive && x.NextDueDate <= thirtyDaysFromToday)
                .OrderBy(x => x.NextDueDate)
                .Take(6)
                .Select(x => new DashboardViewModel.MaintenanceSummaryItem(
                    x.Id, x.Title, x.Property.Name, x.Appliance == null ? null : x.Appliance.Name,
                    x.NextDueDate, x.NextDueDate < today))
                .ToListAsync(cancellationToken);

            var propertyRows = await db.Properties.AsNoTracking()
                .OrderBy(x => x.Name)
                .Take(8)
                .Select(x => new { x.Id, x.Name, x.City, x.Province, RoomCount = x.Rooms.Count, ApplianceCount = x.Appliances.Count, OpenMaintenanceCount = x.MaintenanceTasks.Count(task => task.IsActive && task.NextDueDate <= thirtyDaysFromToday) })
                .ToListAsync(cancellationToken);
            var properties = propertyRows.Select(x => new DashboardViewModel.PropertySummaryItem(
                x.Id, x.Name, string.Join(", ", new[] { x.City, x.Province }.Where(value => !string.IsNullOrWhiteSpace(value))),
                x.RoomCount, x.ApplianceCount, x.OpenMaintenanceCount)).ToList();

            var warrantyEntities = await db.Appliances.AsNoTracking()
                .Where(x => x.WarrantyEndDate != null && x.WarrantyEndDate <= warrantyCutoff)
                .OrderBy(x => x.WarrantyEndDate)
                .Take(8)
                .Select(x => new { x.Id, x.Name, PropertyName = x.Property.Name, x.WarrantyEndDate })
                .ToListAsync(cancellationToken);

            var warrantyAlerts = warrantyEntities.Select(x =>
            {
                var days = x.WarrantyEndDate!.Value.DayNumber - today.DayNumber;
                return new DashboardViewModel.WarrantyAlertItem(
                    x.Id, x.Name, x.PropertyName, x.WarrantyEndDate, days,
                    days < 0 ? "Expired" : days == 0 ? "Expires today" : $"{days} days left");
            }).ToList();

            var expenseRows = await db.MaintenanceExpenses.AsNoTracking()
                .Where(x => x.ExpenseDateUtc >= last30Days)
                .OrderByDescending(x => x.ExpenseDateUtc)
                .Select(x => new { x.Amount, x.ExpenseDateUtc })
                .ToListAsync(cancellationToken);

            var expenseSummary = expenseRows
                .GroupBy(x => new { x.ExpenseDateUtc.Year, x.ExpenseDateUtc.Month })
                .OrderByDescending(group => group.Key.Year * 100 + group.Key.Month)
                .Take(3)
                .Select(group => new DashboardViewModel.ExpenseSummaryItem(
                    new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMM yyyy"), group.Sum(x => x.Amount)))
                .ToList();

            model = new DashboardViewModel
            {
                HouseholdCount = await db.Households.CountAsync(cancellationToken),
                ResidentCount = await db.Residents.CountAsync(cancellationToken),
                ActiveCaregiverCount = await db.Caregivers.CountAsync(x => x.IsActive, cancellationToken),
                UpcomingVisitCount = await db.CareVisits.CountAsync(x => x.ScheduledAtUtc >= DateTime.UtcNow && x.Status != Domain.Entities.CareVisitStatus.Cancelled, cancellationToken),
                UpcomingMaintenance = maintenance,
                Properties = properties,
                WarrantyAlerts = warrantyAlerts,
                ExpenseSummary = expenseSummary,
                ExpenseTotalLast30Days = expenseRows.Sum(x => x.Amount),
                MaintenanceDueCount = maintenance.Count,
                WarrantyAlertCount = warrantyAlerts.Count,
                DatabaseAvailable = true
            };
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Dashboard metrics could not be loaded from PostgreSQL.");
        }

        return View(model);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
