using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class ServiceProvider
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string BusinessName { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string ContactName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = string.Empty;
    [MaxLength(40)] public string? PhoneNumber { get; set; }
    [MaxLength(1200)] public string? Description { get; set; }
    [MaxLength(120)] public string? Trade { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ServiceArea> ServiceAreas { get; set; } = new List<ServiceArea>();
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
