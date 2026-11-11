using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Resident
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string FullName { get; set; } = string.Empty;
    [MaxLength(40)] public string? ContactNumber { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
    [MaxLength(120)] public string? EmergencyContactName { get; set; }
    [MaxLength(80)] public string? EmergencyContactRelationship { get; set; }
    [MaxLength(40)] public string? EmergencyContactNumber { get; set; }
    [MaxLength(2000)] public string? CareRequirements { get; set; }
    [MaxLength(4000)] public string? CareNotes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ArchivedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
