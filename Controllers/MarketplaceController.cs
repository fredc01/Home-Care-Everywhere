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
public class MarketplaceController(ApplicationDbContext db, HouseholdAccessService access) : Controller
{
    public async Task<IActionResult> Providers(CancellationToken cancellationToken)
    {
        var providers = await db.ServiceProviders.Where(x => x.IsActive).OrderBy(x => x.BusinessName).Include(x => x.ServiceAreas).ToListAsync(cancellationToken);
        return View(providers);
    }

    public IActionResult CreateProvider() => View(new CreateProviderViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProvider(CreateProviderViewModel model, CancellationToken cancellationToken)
    {
        model.BusinessName = model.BusinessName?.Trim() ?? string.Empty;
        model.ContactName = model.ContactName?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.PhoneNumber = model.PhoneNumber?.Trim();
        model.Trade = model.Trade?.Trim();
        model.Description = model.Description?.Trim();
        if (!ModelState.IsValid) return View(model);
        var provider = new HomeCarePH.Domain.Entities.ServiceProvider { BusinessName = model.BusinessName, ContactName = model.ContactName, Email = model.Email, PhoneNumber = model.PhoneNumber, Trade = model.Trade, Description = model.Description };
        try
        {
            db.ServiceProviders.Add(provider);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            ModelState.AddModelError(string.Empty, "The provider could not be saved. Check that the database migrations are up to date and try again.");
            HttpContext.RequestServices.GetRequiredService<ILogger<MarketplaceController>>().LogError(exception, "Could not save service provider {BusinessName}", model.BusinessName);
            return View(model);
        }
        TempData["ProviderMessage"] = $"{provider.BusinessName} was added to the service marketplace.";
        return RedirectToAction(nameof(ProviderDetails), new { id = provider.Id });
    }

    public async Task<IActionResult> ProviderDetails(int id, CancellationToken cancellationToken)
    {
        var provider = await db.ServiceProviders.Include(x => x.ServiceAreas).Include(x => x.Quotations).ThenInclude(x => x.RepairRequest).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return provider is null ? NotFound() : View(provider);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddServiceArea(AddServiceAreaViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(ProviderDetails), new { id = model.ServiceProviderId });
        if (!await db.ServiceProviders.AnyAsync(x => x.Id == model.ServiceProviderId, cancellationToken)) return NotFound();
        db.ServiceAreas.Add(new ServiceArea { ServiceProviderId = model.ServiceProviderId, City = model.City.Trim(), Province = model.Province?.Trim(), PostalCode = model.PostalCode?.Trim(), RadiusKm = model.RadiusKm });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(ProviderDetails), new { id = model.ServiceProviderId });
    }

    public async Task<IActionResult> Repairs(int? propertyId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.RepairRequests.Where(x => x.Property.Household.Memberships.Any(m => m.UserId == userId));
        if (propertyId.HasValue) query = query.Where(x => x.PropertyId == propertyId.Value);
        var requests = await query.OrderByDescending(x => x.CreatedAtUtc).Select(x => new RepairListItemViewModel { Id = x.Id, PropertyId = x.PropertyId, PropertyName = x.Property.Name, Title = x.Title, Priority = x.Priority, Status = x.Status, QuotationCount = x.Quotations.Count, CreatedAtUtc = x.CreatedAtUtc }).ToListAsync(cancellationToken);
        ViewData["PropertyId"] = propertyId;
        return View(requests);
    }

    public async Task<IActionResult> CreateRepair(int propertyId, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(propertyId, cancellationToken);
        if (property is null) return Forbid();
        return View(new CreateRepairRequestViewModel { PropertyId = propertyId, Appliances = await db.Appliances.Where(x => x.PropertyId == propertyId).OrderBy(x => x.Name).ToListAsync(cancellationToken) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRepair(CreateRepairRequestViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (model.ApplianceId.HasValue && !await db.Appliances.AnyAsync(x => x.Id == model.ApplianceId && x.PropertyId == model.PropertyId, cancellationToken)) ModelState.AddModelError(nameof(model.ApplianceId), "Select an appliance from this property.");
        if (!ModelState.IsValid) { model.Appliances = await db.Appliances.Where(x => x.PropertyId == model.PropertyId).OrderBy(x => x.Name).ToListAsync(cancellationToken); return View(model); }
        var request = new RepairRequest { PropertyId = model.PropertyId, ApplianceId = model.ApplianceId, Title = model.Title.Trim(), Description = model.Description.Trim(), Priority = model.Priority, PreferredDate = model.PreferredDate, RequestedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)! };
        db.RepairRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(RepairDetails), new { id = request.Id });
    }

    public async Task<IActionResult> RepairDetails(int id, CancellationToken cancellationToken)
    {
        var request = await db.RepairRequests.Include(x => x.Property).ThenInclude(x => x.Household).Include(x => x.Appliance).Include(x => x.RequestedByUser).Include(x => x.Quotations).ThenInclude(x => x.ServiceProvider).Include(x => x.Bookings).ThenInclude(x => x.ServiceProvider).Include(x => x.Feedback).ThenInclude(x => x.ServiceProvider).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (request is null) return NotFound();
        var role = await access.GetRoleAsync(User, request.Property.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        ViewData["CanEdit"] = HouseholdAccessService.CanEditProperty(role.Value);
        ViewData["Providers"] = await db.ServiceProviders.Where(x => x.IsActive).OrderBy(x => x.BusinessName).ToListAsync(cancellationToken);
        return View(request);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuote(SubmitQuotationViewModel model, CancellationToken cancellationToken)
    {
        var request = await db.RepairRequests.Include(x => x.Property).SingleOrDefaultAsync(x => x.Id == model.RepairRequestId, cancellationToken);
        if (request is null) return NotFound();
        if (!await db.ServiceProviders.AnyAsync(x => x.Id == model.ServiceProviderId && x.IsActive, cancellationToken)) return NotFound();
        if (!ModelState.IsValid) return RedirectToAction(nameof(RepairDetails), new { id = model.RepairRequestId });
        db.Quotations.Add(new Quotation { RepairRequestId = model.RepairRequestId, ServiceProviderId = model.ServiceProviderId, Amount = model.Amount, Description = model.Description.Trim(), ValidUntilUtc = model.ValidUntilUtc.ToUniversalTime() });
        request.Status = RepairRequestStatus.Quoted;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(RepairDetails), new { id = model.RepairRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptQuote(AcceptQuotationViewModel model, CancellationToken cancellationToken)
    {
        var quote = await db.Quotations.Include(x => x.RepairRequest).ThenInclude(x => x.Property).SingleOrDefaultAsync(x => x.Id == model.QuotationId, cancellationToken);
        if (quote is null) return NotFound();
        var role = await access.GetRoleAsync(User, quote.RepairRequest.Property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (quote.ValidUntilUtc < DateTime.UtcNow) { TempData["RepairMessage"] = "That quotation has expired."; return RedirectToAction(nameof(RepairDetails), new { id = quote.RepairRequestId }); }
        await db.Quotations.Where(x => x.RepairRequestId == quote.RepairRequestId && x.Id != quote.Id).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, QuotationStatus.Declined), cancellationToken);
        quote.Status = QuotationStatus.Accepted;
        db.Bookings.Add(new Booking { RepairRequestId = quote.RepairRequestId, ServiceProviderId = quote.ServiceProviderId, QuotationId = quote.Id, ScheduledStartUtc = model.ScheduledStartUtc.ToUniversalTime(), Status = BookingStatus.Confirmed, Notes = model.Notes?.Trim() });
        quote.RepairRequest.Status = RepairRequestStatus.Booked;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(RepairDetails), new { id = quote.RepairRequestId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFeedback(AddFeedbackViewModel model, CancellationToken cancellationToken)
    {
        var request = await db.RepairRequests.Include(x => x.Property).SingleOrDefaultAsync(x => x.Id == model.RepairRequestId, cancellationToken);
        if (request is null) return NotFound();
        var role = await access.GetRoleAsync(User, request.Property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (!ModelState.IsValid) return RedirectToAction(nameof(RepairDetails), new { id = model.RepairRequestId });
        db.RepairFeedback.Add(new RepairFeedback { RepairRequestId = model.RepairRequestId, ServiceProviderId = model.ServiceProviderId, SubmittedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!, Rating = model.Rating, Comments = model.Comments?.Trim() });
        request.Status = RepairRequestStatus.Completed;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(RepairDetails), new { id = model.RepairRequestId });
    }

    private async Task<Property?> GetPropertyForUser(int propertyId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await db.Properties.AsNoTracking().SingleOrDefaultAsync(x => x.Id == propertyId && x.Household.Memberships.Any(m => m.UserId == userId), cancellationToken);
    }
}
