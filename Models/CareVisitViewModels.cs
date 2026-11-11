using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class CareVisitListViewModel
{
    public IReadOnlyList<CareVisitListItemViewModel> Visits { get; init; } = [];
    public bool History { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 15;
    public int TotalCount { get; init; }
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
public class CareVisitListItemViewModel
{
    public int Id { get; init; }
    public string ResidentName { get; init; } = string.Empty;
    public string HouseholdName { get; init; } = string.Empty;
    public string CaregiverName { get; init; } = string.Empty;
    public DateTime ScheduledAtUtc { get; init; }
    public int ExpectedDurationMinutes { get; init; }
    public CareVisitStatus Status { get; init; }
    public bool IsConflict { get; init; }
}
public class CareVisitFormViewModel
{
    public int? Id { get; set; }
    [Required, Display(Name = "Resident")] public int ResidentId { get; set; }
    [Required, Display(Name = "Caregiver")] public int CaregiverId { get; set; }
    [Required, Display(Name = "Visit date and start time")] public DateTime ScheduledAtLocal { get; set; } = DateTime.Now.AddHours(1);
    [Range(15, 1440), Display(Name = "Expected duration (minutes)")] public int ExpectedDurationMinutes { get; set; } = 60;
    [Required] public CareVisitStatus Status { get; set; } = CareVisitStatus.Scheduled;
    [StringLength(1200), Display(Name = "Instructions")] public string? Instructions { get; set; }
    [StringLength(500), Display(Name = "Visit notes")] public string? Notes { get; set; }
    [StringLength(2000), Display(Name = "Completion details")] public string? CompletionNotes { get; set; }
    public IReadOnlyList<CareVisitResidentOption> Residents { get; init; } = [];
    public IReadOnlyList<CareVisitCaregiverOption> Caregivers { get; init; } = [];
    public string? ConflictMessage { get; set; }
}
public record CareVisitResidentOption(int Id, string Name, string HouseholdName);
public record CareVisitCaregiverOption(int Id, string Name, string? Role);
public class CareVisitDetailsViewModel
{
    public CareVisit Visit { get; init; } = null!;
    public bool HasConflict { get; init; }
}
