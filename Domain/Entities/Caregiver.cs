using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Caregiver
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Role { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(40)]
    public string? ContactNumber { get; set; }

    [MaxLength(1200)] public string? Skills { get; set; }
    [MaxLength(1200)] public string? Qualifications { get; set; }
    [MaxLength(1200)] public string? Specializations { get; set; }
    [MaxLength(1200)] public string? Availability { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<CaregiverAssignment> Assignments { get; set; } = new List<CaregiverAssignment>();
}
