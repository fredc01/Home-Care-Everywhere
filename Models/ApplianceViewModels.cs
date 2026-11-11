using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class ApplianceListItemViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public string? ModelNumber { get; init; }
    public string? SerialNumber { get; init; }
    public DateOnly? WarrantyEndDate { get; init; }
    public int DocumentCount { get; init; }
}

public class CreateApplianceViewModel
{
    public int PropertyId { get; set; }
    [Required, StringLength(140)] public string Name { get; set; } = string.Empty;
    [StringLength(120)] public string? Brand { get; set; }
    [StringLength(120)] [Display(Name = "Model number")] public string? ModelNumber { get; set; }
    [StringLength(120)] [Display(Name = "Serial number")] public string? SerialNumber { get; set; }
    [Display(Name = "Purchase date")] public DateOnly? PurchaseDate { get; set; }
    [DataType(DataType.Currency), Range(0, 999999999)] [Display(Name = "Purchase price")] public decimal? PurchasePrice { get; set; }
    [Display(Name = "Warranty starts")] public DateOnly? WarrantyStartDate { get; set; }
    [Display(Name = "Warranty ends")] public DateOnly? WarrantyEndDate { get; set; }
    [StringLength(1200)] public string? Notes { get; set; }
}

public class UploadApplianceDocumentViewModel
{
    public int ApplianceId { get; set; }
    [Required] public IFormFile Document { get; set; } = null!;
    [Required] [Display(Name = "Document type")] public ApplianceDocumentType DocumentType { get; set; } = ApplianceDocumentType.Warranty;
}

public class ApplianceDetailsViewModel
{
    public Appliance Appliance { get; init; } = null!;
    public HouseholdRole CurrentRole { get; init; }
    public UploadApplianceDocumentViewModel Upload { get; init; } = new();
}
