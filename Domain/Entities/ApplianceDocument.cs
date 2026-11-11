using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public enum ApplianceDocumentType
{
    Warranty,
    Receipt,
    Manual,
    Other
}

public class ApplianceDocument
{
    public int Id { get; set; }
    public int ApplianceId { get; set; }
    public Appliance Appliance { get; set; } = null!;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string StoredPath { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? ContentType { get; set; }

    public long FileSizeBytes { get; set; }
    public ApplianceDocumentType DocumentType { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
