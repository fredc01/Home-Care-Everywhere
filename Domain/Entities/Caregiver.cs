using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Caregiver
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Role { get; set; }

    [MaxLength(40)]
    public string? ContactNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
