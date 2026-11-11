using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class HouseholdInvitation
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }
    public Household Household { get; set; } = null!;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public HouseholdRole Role { get; set; } = HouseholdRole.Member;

    [Required, MaxLength(96)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string InvitedByUserId { get; set; } = string.Empty;
    public ApplicationUser InvitedByUser { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsPending => AcceptedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
