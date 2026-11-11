using System.ComponentModel.DataAnnotations;
using HomeCarePH.Domain.Entities;

namespace HomeCarePH.Models;

public class MaintenanceListItemViewModel
{
    public int Id { get; init; }
    public int PropertyId { get; init; }
    public string PropertyName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? ApplianceName { get; init; }
    public MaintenanceFrequency Frequency { get; init; }
    public DateOnly NextDueDate { get; init; }
    public bool IsActive { get; init; }
    public int CompletionCount { get; init; }
}

public class CreateMaintenanceViewModel
{
    public int PropertyId { get; set; }
    [Required, StringLength(160)] public string Title { get; set; } = string.Empty;
    [StringLength(1200)] public string? Description { get; set; }
    [Display(Name = "Appliance")] public int? ApplianceId { get; set; }
    [Required] public MaintenanceFrequency Frequency { get; set; } = MaintenanceFrequency.Monthly;
    [Range(1, 3650)] [Display(Name = "Custom interval (days)")] public int? CustomIntervalDays { get; set; }
    [Required] [Display(Name = "First due date")] public DateOnly NextDueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public IReadOnlyList<Appliance> Appliances { get; set; } = [];
}

public class CompleteMaintenanceViewModel
{
    public int MaintenanceTaskId { get; set; }
    [StringLength(1200)] public string? Notes { get; set; }
    [Range(0, 999999999)] [Display(Name = "Expense amount")] public decimal? ExpenseAmount { get; set; }
    [StringLength(160)] public string? Vendor { get; set; }
    [StringLength(300)] [Display(Name = "Expense description")] public string? ExpenseDescription { get; set; }
}

public class MaintenanceDetailsViewModel
{
    public MaintenanceTask Task { get; init; } = null!;
    public HouseholdRole CurrentRole { get; init; }
    public CompleteMaintenanceViewModel Completion { get; init; } = new();
}
