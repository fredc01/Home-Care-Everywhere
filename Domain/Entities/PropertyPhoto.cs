using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class PropertyPhoto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string StoredPath { get; set; } = string.Empty;

    [MaxLength(180)]
    public string? Caption { get; set; }

    [MaxLength(120)]
    public string? ContentType { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
