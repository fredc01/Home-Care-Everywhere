using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Property
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PropertyType { get; set; }

    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(1200)]
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<RoomSpace> Rooms { get; set; } = new List<RoomSpace>();
    public ICollection<PropertyPhoto> Photos { get; set; } = new List<PropertyPhoto>();
    public ICollection<PropertyRecord> Records { get; set; } = new List<PropertyRecord>();
    public ICollection<Appliance> Appliances { get; set; } = new List<Appliance>();
}
