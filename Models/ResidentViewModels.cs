using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class ResidentListViewModel
{
    public IReadOnlyList<ResidentListItemViewModel> Residents { get; init; } = [];
    public IReadOnlyList<HouseholdFilterOption> Households { get; init; } = [];
    public string Search { get; init; } = string.Empty;
    public int? HouseholdId { get; init; }
    public bool IncludeArchived { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 12;
    public int TotalCount { get; init; }
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public class ResidentListItemViewModel
{
    public int Id { get; init; }
    public int HouseholdId { get; init; }
    public string HouseholdName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? ContactNumber { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public bool IsActive { get; init; }
    public int VisitCount { get; init; }
    public DateTime? NextVisitUtc { get; init; }
}

public record HouseholdFilterOption(int Id, string Name);

public class ResidentFormViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [Phone, StringLength(40), Display(Name = "Contact number")] public string? ContactNumber { get; set; }
    [Display(Name = "Date of birth")] public DateOnly? DateOfBirth { get; set; }
    [Required, Display(Name = "Household")] public int HouseholdId { get; set; }
    [StringLength(120), Display(Name = "Emergency contact")] public string? EmergencyContactName { get; set; }
    [StringLength(80), Display(Name = "Relationship")] public string? EmergencyContactRelationship { get; set; }
    [Phone, StringLength(40), Display(Name = "Emergency phone")] public string? EmergencyContactNumber { get; set; }
    [StringLength(2000), Display(Name = "Care requirements")] public string? CareRequirements { get; set; }
    [StringLength(4000), Display(Name = "Care notes")] public string? CareNotes { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<HouseholdFilterOption> Households { get; init; } = [];
}

public class ResidentDetailsViewModel
{
    public Resident Resident { get; init; } = null!;
    public HouseholdRole CurrentRole { get; init; }
    public IReadOnlyList<CareVisit> CareHistory { get; init; } = [];
}
