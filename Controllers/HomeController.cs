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
        var model = new DashboardViewModel();

        try
        {
            model = new DashboardViewModel
            {
                HouseholdCount = await db.Households.CountAsync(cancellationToken),
                ResidentCount = await db.Residents.CountAsync(cancellationToken),
                ActiveCaregiverCount = await db.Caregivers.CountAsync(x => x.IsActive, cancellationToken),
                UpcomingVisitCount = await db.CareVisits.CountAsync(x => x.ScheduledAtUtc >= DateTime.UtcNow && x.Status != Domain.Entities.CareVisitStatus.Cancelled, cancellationToken),
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
