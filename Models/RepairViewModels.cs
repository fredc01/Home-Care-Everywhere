using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class CreateProviderViewModel
{
    [Required, StringLength(160)] [Display(Name = "Business name")] public string BusinessName { get; set; } = string.Empty;
    [Required, StringLength(120)] [Display(Name = "Contact name")] public string ContactName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Phone] [Display(Name = "Phone number")] public string? PhoneNumber { get; set; }
    [StringLength(120)] public string? Trade { get; set; }
    [StringLength(1200)] public string? Description { get; set; }
}

public class AddServiceAreaViewModel
{
    public int ServiceProviderId { get; set; }
    [Required, StringLength(100)] public string City { get; set; } = string.Empty;
    [StringLength(100)] public string? Province { get; set; }
    [StringLength(20)] [Display(Name = "Postal code")] public string? PostalCode { get; set; }
    [Range(0, 500)] [Display(Name = "Radius (km)")] public int? RadiusKm { get; set; }
}

public class CreateRepairRequestViewModel
{
    public int PropertyId { get; set; }
    [Display(Name = "Appliance")] public int? ApplianceId { get; set; }
    [Required, StringLength(160)] public string Title { get; set; } = string.Empty;
    [Required, StringLength(2500)] public string Description { get; set; } = string.Empty;
    [Required] public RepairRequestPriority Priority { get; set; } = RepairRequestPriority.Normal;
    [Display(Name = "Preferred date")] public DateOnly? PreferredDate { get; set; }
    public IReadOnlyList<Appliance> Appliances { get; set; } = [];
}

public class SubmitQuotationViewModel
{
    public int RepairRequestId { get; set; }
    public int ServiceProviderId { get; set; }
    [Range(0, 999999999)] public decimal Amount { get; set; }
    [Required, StringLength(1200)] public string Description { get; set; } = string.Empty;
    [Required] [Display(Name = "Valid until")] public DateTime ValidUntilUtc { get; set; } = DateTime.UtcNow.AddDays(14);
}

public class AcceptQuotationViewModel
{
    public int QuotationId { get; set; }
    [Required] [Display(Name = "Scheduled start")] public DateTime ScheduledStartUtc { get; set; } = DateTime.UtcNow.AddDays(1);
    [StringLength(1200)] public string? Notes { get; set; }
}

public class AddFeedbackViewModel
{
    public int RepairRequestId { get; set; }
    public int ServiceProviderId { get; set; }
    [Range(1, 5)] public int Rating { get; set; } = 5;
    [StringLength(1200)] public string? Comments { get; set; }
}

public class RepairListItemViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public RepairRequestPriority Priority { get; init; }
    public RepairRequestStatus Status { get; init; }
    public int QuotationCount { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
