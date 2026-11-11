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
public class AppliancesController(ApplicationDbContext db, HouseholdAccessService access, IWebHostEnvironment environment, ILogger<AppliancesController> logger) : Controller
{
    private static readonly string[] AllowedDocumentTypes = ["application/pdf", "image/jpeg", "image/png", "image/webp", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];
    private const long MaxDocumentBytes = 15 * 1024 * 1024;

    public async Task<IActionResult> Index(int propertyId, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(propertyId, cancellationToken);
        if (property is null) return Forbid();
        var appliances = await db.Appliances.Where(x => x.PropertyId == propertyId).OrderBy(x => x.Name).Select(x => new ApplianceListItemViewModel { Id = x.Id, PropertyId = x.PropertyId, PropertyName = x.Property.Name, Name = x.Name, Brand = x.Brand, ModelNumber = x.ModelNumber, SerialNumber = x.SerialNumber, WarrantyEndDate = x.WarrantyEndDate, DocumentCount = x.Documents.Count }).ToListAsync(cancellationToken);
        ViewData["PropertyName"] = property.Name;
        ViewData["HouseholdId"] = property.HouseholdId;
        return View(appliances);
    }

    public async Task<IActionResult> Create(int propertyId, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(propertyId, cancellationToken);
        if (property is null) return Forbid();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        ViewData["PropertyName"] = property.Name;
        return View(new CreateApplianceViewModel { PropertyId = propertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateApplianceViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetPropertyForUser(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (model.WarrantyStartDate.HasValue && model.WarrantyEndDate.HasValue && model.WarrantyEndDate < model.WarrantyStartDate) ModelState.AddModelError(nameof(model.WarrantyEndDate), "Warranty end date must be after the start date.");
        if (!ModelState.IsValid) { ViewData["PropertyName"] = property.Name; return View(model); }
        var appliance = new Appliance { PropertyId = model.PropertyId, Name = model.Name.Trim(), Brand = model.Brand?.Trim(), ModelNumber = model.ModelNumber?.Trim(), SerialNumber = model.SerialNumber?.Trim(), PurchaseDate = model.PurchaseDate, PurchasePrice = model.PurchasePrice, WarrantyStartDate = model.WarrantyStartDate, WarrantyEndDate = model.WarrantyEndDate, Notes = model.Notes?.Trim() };
        db.Appliances.Add(appliance);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = appliance.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var appliance = await db.Appliances.Include(x => x.Property).ThenInclude(x => x.Household).Include(x => x.Documents.OrderByDescending(d => d.UploadedAtUtc)).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (appliance is null) return NotFound();
        var role = await access.GetRoleAsync(User, appliance.Property.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        return View(new ApplianceDetailsViewModel { Appliance = appliance, CurrentRole = role.Value, Upload = new UploadApplianceDocumentViewModel { ApplianceId = id } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDocument([Bind(Prefix = "Upload")] UploadApplianceDocumentViewModel model, CancellationToken cancellationToken)
    {
        var appliance = await db.Appliances.Include(x => x.Property).SingleOrDefaultAsync(x => x.Id == model.ApplianceId, cancellationToken);
        if (appliance is null) return NotFound();
        var role = await access.GetRoleAsync(User, appliance.Property.HouseholdId, cancellationToken);
        if (role is null || !HouseholdAccessService.CanEditProperty(role.Value)) return Forbid();
        if (model.Document is null || model.Document.Length == 0) { TempData["ApplianceMessage"] = "Choose a document before uploading."; return RedirectToAction(nameof(Details), new { id = model.ApplianceId }); }
        if (model.Document.Length > MaxDocumentBytes || !AllowedDocumentTypes.Contains(model.Document.ContentType, StringComparer.OrdinalIgnoreCase)) { TempData["ApplianceMessage"] = "Documents must be PDF, JPG, PNG, WebP, or DOCX files no larger than 15 MB."; return RedirectToAction(nameof(Details), new { id = model.ApplianceId }); }
        var extension = Path.GetExtension(model.Document.FileName).ToLowerInvariant();
        var relativeDirectory = Path.Combine("uploads", "appliances", appliance.Id.ToString());
        var physicalDirectory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(physicalDirectory);
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(physicalDirectory, safeName);
        await using (var stream = System.IO.File.Create(physicalPath)) await model.Document.CopyToAsync(stream, cancellationToken);
        var storedPath = "/" + relativeDirectory.Replace(Path.DirectorySeparatorChar, '/') + "/" + safeName;
        db.ApplianceDocuments.Add(new ApplianceDocument { ApplianceId = appliance.Id, FileName = Path.GetFileName(model.Document.FileName), StoredPath = storedPath, ContentType = model.Document.ContentType, FileSizeBytes = model.Document.Length, DocumentType = model.DocumentType });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Uploaded {DocumentType} document for appliance {ApplianceId} as {StoredPath}", model.DocumentType, appliance.Id, safeName);
        return RedirectToAction(nameof(Details), new { id = model.ApplianceId });
    }

    private async Task<Property?> GetPropertyForUser(int propertyId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await db.Properties.AsNoTracking().SingleOrDefaultAsync(x => x.Id == propertyId && x.Household.Memberships.Any(m => m.UserId == userId), cancellationToken);
    }
}
