namespace HomeCarePH.Models;

public class DashboardViewModel
{
    public int HouseholdCount { get; init; }
    public int ResidentCount { get; init; }
    public int ActiveCaregiverCount { get; init; }
    public int UpcomingVisitCount { get; init; }
    public bool DatabaseAvailable { get; init; }
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<MaintenanceSummaryItem> UpcomingMaintenance { get; init; } = [];
    public IReadOnlyList<PropertySummaryItem> Properties { get; init; } = [];
    public IReadOnlyList<WarrantyAlertItem> WarrantyAlerts { get; init; } = [];
    public IReadOnlyList<ExpenseSummaryItem> ExpenseSummary { get; init; } = [];
    public decimal ExpenseTotalLast30Days { get; init; }
    public int MaintenanceDueCount { get; init; }
    public int WarrantyAlertCount { get; init; }

    public sealed record MaintenanceSummaryItem(int Id, string Title, string PropertyName, string? ApplianceName, DateOnly DueDate, bool IsOverdue);
    public sealed record PropertySummaryItem(int Id, string Name, string Location, int RoomCount, int ApplianceCount, int OpenMaintenanceCount);
    public sealed record WarrantyAlertItem(int Id, string ApplianceName, string PropertyName, DateOnly? WarrantyEndDate, int DaysRemaining, string Status);
    public sealed record ExpenseSummaryItem(string Label, decimal Amount);
}
