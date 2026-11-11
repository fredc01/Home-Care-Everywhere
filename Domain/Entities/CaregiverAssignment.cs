using System.ComponentModel.DataAnnotations;
namespace HomeCarePH.Domain.Entities;

public class CaregiverAssignment
{
    public int Id { get; set; }
    public int CaregiverId { get; set; }
    public Caregiver Caregiver { get; set; } = null!;
    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
    public int? ResidentId { get; set; }
    public Resident? Resident { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? EndedAtUtc { get; set; }
}
