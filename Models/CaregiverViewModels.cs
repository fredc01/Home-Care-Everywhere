using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class CaregiverListViewModel
{
    public IReadOnlyList<CaregiverListItemViewModel> Caregivers { get; init; } = [];
    public string Search { get; init; } = string.Empty;
    public bool IncludeInactive { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 12;
    public int TotalCount { get; init; }
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
public class CaregiverListItemViewModel
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Role { get; init; }
    public string? ContactNumber { get; init; }
    public bool IsActive { get; init; }
    public int AssignmentCount { get; init; }
    public int VisitCount { get; init; }
    public DateTime? NextVisitUtc { get; init; }
}
public record CareTeamHouseholdOption(int Id, string Name);
public record CareTeamResidentOption(int Id, int HouseholdId, string Name);
public class CaregiverFormViewModel
{
    public int? Id { get; set; }
    [Required, StringLength(120)] public string FullName { get; set; } = string.Empty;
    [StringLength(120)] public string? Role { get; set; }
    [EmailAddress, StringLength(256)] public string? Email { get; set; }
    [Phone, StringLength(40)] public string? ContactNumber { get; set; }
    [StringLength(1200)] public string? Skills { get; set; }
    [StringLength(1200)] public string? Qualifications { get; set; }
    [StringLength(1200)] public string? Specializations { get; set; }
    [StringLength(1200)] public string? Availability { get; set; }
    [StringLength(2000)] public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    [Required, Display(Name = "Initial household assignment")] public int HouseholdId { get; set; }
    public int? ResidentId { get; set; }
    public IReadOnlyList<CareTeamHouseholdOption> Households { get; init; } = [];
    public IReadOnlyList<CareTeamResidentOption> Residents { get; init; } = [];
}
public class CaregiverDetailsViewModel
{
    public Caregiver Caregiver { get; init; } = null!;
    public IReadOnlyList<CaregiverAssignment> Assignments { get; init; } = [];
    public IReadOnlyList<CareVisit> UpcomingVisits { get; init; } = [];
    public IReadOnlyList<CareVisit> PreviousVisits { get; init; } = [];
    public IReadOnlyList<CareTeamHouseholdOption> Households { get; init; } = [];
    public IReadOnlyList<CareTeamResidentOption> Residents { get; init; } = [];
}
public class CaregiverAssignmentFormViewModel
{
    public int CaregiverId { get; set; }
    [Required] public int HouseholdId { get; set; }
    public int? ResidentId { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    public IReadOnlyList<CareTeamHouseholdOption> Households { get; init; } = [];
    public IReadOnlyList<CareTeamResidentOption> Residents { get; init; } = [];
}
