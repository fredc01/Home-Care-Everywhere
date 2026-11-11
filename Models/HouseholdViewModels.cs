using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class HouseholdListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public HouseholdRole Role { get; init; }
    public int MemberCount { get; init; }
}

public class CreateHouseholdViewModel
{
    [Required, StringLength(150)]
    [Display(Name = "Household name")]
    public string Name { get; set; } = string.Empty;
}

public class InviteMemberViewModel
{
    public int HouseholdId { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public HouseholdRole Role { get; set; } = HouseholdRole.Member;
}

public class HouseholdDetailsViewModel
{
    public Household Household { get; init; } = null!;
    public HouseholdRole CurrentRole { get; init; }
    public IReadOnlyList<HouseholdMembership> Members { get; init; } = [];
    public IReadOnlyList<HouseholdInvitation> PendingInvitations { get; init; } = [];
    public InviteMemberViewModel Invite { get; init; } = new();
}
