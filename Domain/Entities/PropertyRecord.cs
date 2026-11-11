using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class PropertyRecord
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? RecordType { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
}
