using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public enum HouseholdRole
{
    Owner,
    Admin,
    Member,
    Viewer
}

public class HouseholdMembership
{
    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public HouseholdRole Role { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}
