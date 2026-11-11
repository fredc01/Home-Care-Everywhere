using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class ServiceArea
{
    public int Id { get; set; }
    public int ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;
    [Required, MaxLength(100)] public string City { get; set; } = string.Empty;
    [MaxLength(100)] public string? Province { get; set; }
    [MaxLength(20)] public string? PostalCode { get; set; }
    [Range(0, 500)] public int? RadiusKm { get; set; }
}
