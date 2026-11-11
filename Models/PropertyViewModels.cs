using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class PropertyListItemViewModel
{
    public int Id { get; init; }
    public int HouseholdId { get; init; }
    public string HouseholdName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? PropertyType { get; init; }
    public string? City { get; init; }
    public int RoomCount { get; init; }
    public int PhotoCount { get; init; }
}

public class CreatePropertyViewModel
{
    public int HouseholdId { get; set; }
    [Required, StringLength(150)]
    [Display(Name = "Property name")]
    public string Name { get; set; } = string.Empty;
    [StringLength(100)]
    [Display(Name = "Property type")]
    public string? PropertyType { get; set; }
    [StringLength(200)]
    [Display(Name = "Address")]
    public string? AddressLine1 { get; set; }
    [StringLength(100)] public string? City { get; set; }
    [StringLength(100)] public string? Province { get; set; }
    [StringLength(20)] [Display(Name = "Postal code")] public string? PostalCode { get; set; }
    [StringLength(1200)] public string? Description { get; set; }
}

public class AddRoomViewModel
{
    public int PropertyId { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [StringLength(80)] [Display(Name = "Space type")] public string? SpaceType { get; set; }
    [StringLength(500)] public string? Description { get; set; }
}

public class AddPropertyRecordViewModel
{
    public int PropertyId { get; set; }
    [Required, StringLength(160)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(4000)] public string Notes { get; set; } = string.Empty;
    [StringLength(80)] [Display(Name = "Record type")] public string? RecordType { get; set; }
}

public class UploadPropertyPhotoViewModel
{
    public int PropertyId { get; set; }
    [Required] public IFormFile Photo { get; set; } = null!;
    [StringLength(180)] public string? Caption { get; set; }
}

public class PropertyDetailsViewModel
{
    public Property Property { get; init; } = null!;
    public HouseholdRole CurrentRole { get; init; }
    public AddRoomViewModel Room { get; init; } = new();
    public AddPropertyRecordViewModel Record { get; init; } = new();
    public UploadPropertyPhotoViewModel Upload { get; init; } = new();
}
