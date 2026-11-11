using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HomeCarePH.Domain.Entities
{
    public class Household
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Resident> Residents { get; set; } = new List<Resident>();

        public ICollection<HouseholdMembership> Memberships { get; set; } = new List<HouseholdMembership>();

        public ICollection<HouseholdInvitation> Invitations { get; set; } = new List<HouseholdInvitation>();
    }
}
