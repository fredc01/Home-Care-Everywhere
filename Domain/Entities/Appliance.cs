using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class Appliance
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    [Required, MaxLength(140)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Brand { get; set; }

    [MaxLength(120)]
    public string? ModelNumber { get; set; }

    [MaxLength(120)]
    public string? SerialNumber { get; set; }

    public DateOnly? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateOnly? WarrantyStartDate { get; set; }
    public DateOnly? WarrantyEndDate { get; set; }

    [MaxLength(1200)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApplianceDocument> Documents { get; set; } = new List<ApplianceDocument>();
}
