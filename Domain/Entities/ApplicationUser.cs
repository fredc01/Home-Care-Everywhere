using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HomeCarePH.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required, MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<HouseholdMembership> HouseholdMemberships { get; set; } = new List<HouseholdMembership>();
}
