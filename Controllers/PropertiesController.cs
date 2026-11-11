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
public class PropertiesController(ApplicationDbContext db, HouseholdAccessService access, IWebHostEnvironment environment, ILogger<PropertiesController> logger) : Controller
{
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxPhotoBytes = 10 * 1024 * 1024;

    public async Task<IActionResult> Index(int? householdId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = db.Properties.Where(x => x.Household.Memberships.Any(m => m.UserId == userId));
        if (householdId.HasValue) query = query.Where(x => x.HouseholdId == householdId.Value);
        var properties = await query.OrderBy(x => x.Name).Select(x => new PropertyListItemViewModel { Id = x.Id, HouseholdId = x.HouseholdId, HouseholdName = x.Household.Name, Name = x.Name, PropertyType = x.PropertyType, City = x.City, RoomCount = x.Rooms.Count, PhotoCount = x.Photos.Count }).ToListAsync(cancellationToken);
        return View(properties);
    }

    public async Task<IActionResult> Create(int householdId, CancellationToken cancellationToken)
    {
        var role = await access.GetRoleAsync(User, householdId, cancellationToken);
        if (role is null) return Forbid();
        if (!HouseholdAccessService.CanManageMembers(role.Value)) return Forbid();
        return View(new CreatePropertyViewModel { HouseholdId = householdId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePropertyViewModel model, CancellationToken cancellationToken)
    {
        var role = await access.GetRoleAsync(User, model.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        if (!HouseholdAccessService.CanManageMembers(role.Value)) return Forbid();
        if (!ModelState.IsValid) return View(model);
        var property = new Property { HouseholdId = model.HouseholdId, Name = model.Name.Trim(), PropertyType = model.PropertyType?.Trim(), AddressLine1 = model.AddressLine1?.Trim(), City = model.City?.Trim(), Province = model.Province?.Trim(), PostalCode = model.PostalCode?.Trim(), Description = model.Description?.Trim() };
        db.Properties.Add(property);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var property = await db.Properties.Include(x => x.Household).Include(x => x.Rooms).Include(x => x.Photos.OrderByDescending(p => p.UploadedAtUtc)).Include(x => x.Records.OrderByDescending(r => r.RecordedAtUtc)).ThenInclude(x => x.CreatedByUser).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (property is null) return NotFound();
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        if (role is null) return Forbid();
        return View(new PropertyDetailsViewModel { Property = property, CurrentRole = role.Value, Room = new AddRoomViewModel { PropertyId = id }, Record = new AddPropertyRecordViewModel { PropertyId = id }, Upload = new UploadPropertyPhotoViewModel { PropertyId = id } });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoom([Bind(Prefix = "Room")] AddRoomViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetAuthorizedProperty(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = model.PropertyId });
        db.RoomSpaces.Add(new RoomSpace { PropertyId = model.PropertyId, Name = model.Name.Trim(), SpaceType = model.SpaceType?.Trim(), Description = model.Description?.Trim() });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecord([Bind(Prefix = "Record")] AddPropertyRecordViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetAuthorizedProperty(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = model.PropertyId });
        db.PropertyRecords.Add(new PropertyRecord { PropertyId = model.PropertyId, Title = model.Title.Trim(), Notes = model.Notes.Trim(), RecordType = model.RecordType?.Trim(), CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)! });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto([Bind(Prefix = "Upload")] UploadPropertyPhotoViewModel model, CancellationToken cancellationToken)
    {
        var property = await GetAuthorizedProperty(model.PropertyId, cancellationToken);
        if (property is null) return Forbid();
        if (model.Photo is null || model.Photo.Length == 0) { TempData["PropertyMessage"] = "Choose a photo before uploading."; return RedirectToAction(nameof(Details), new { id = model.PropertyId }); }
        if (model.Photo.Length > MaxPhotoBytes || !AllowedImageTypes.Contains(model.Photo.ContentType, StringComparer.OrdinalIgnoreCase)) { TempData["PropertyMessage"] = "Photos must be JPG, PNG, or WebP files no larger than 10 MB."; return RedirectToAction(nameof(Details), new { id = model.PropertyId }); }
        var extension = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();
        var relativeDirectory = Path.Combine("uploads", "properties", property.Id.ToString());
        var physicalDirectory = Path.Combine(environment.WebRootPath, relativeDirectory);
        Directory.CreateDirectory(physicalDirectory);
        var safeName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(physicalDirectory, safeName);
        await using (var stream = System.IO.File.Create(physicalPath)) await model.Photo.CopyToAsync(stream, cancellationToken);
        db.PropertyPhotos.Add(new PropertyPhoto { PropertyId = property.Id, FileName = Path.GetFileName(model.Photo.FileName), StoredPath = "/" + relativeDirectory.Replace(Path.DirectorySeparatorChar, '/') + "/" + safeName, Caption = model.Caption?.Trim(), ContentType = model.Photo.ContentType });
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Uploaded property photo for property {PropertyId} as {StoredPath}", property.Id, safeName);
        return RedirectToAction(nameof(Details), new { id = model.PropertyId });
    }

    private async Task<Property?> GetAuthorizedProperty(int propertyId, CancellationToken cancellationToken)
    {
        var property = await db.Properties.AsNoTracking().SingleOrDefaultAsync(x => x.Id == propertyId, cancellationToken);
        if (property is null) return null;
        var role = await access.GetRoleAsync(User, property.HouseholdId, cancellationToken);
        return role is not null && HouseholdAccessService.CanEditProperty(role.Value) ? property : null;
    }
}
