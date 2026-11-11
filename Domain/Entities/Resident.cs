using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Resident
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? ContactNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
