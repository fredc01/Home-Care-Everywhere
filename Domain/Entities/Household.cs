using System;
using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities
{
    public class Household
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}