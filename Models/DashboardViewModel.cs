namespace HomeCarePH.Models;

public class DashboardViewModel
{
    public int HouseholdCount { get; init; }
    public int ResidentCount { get; init; }
    public int ActiveCaregiverCount { get; init; }
    public int UpcomingVisitCount { get; init; }
    public bool DatabaseAvailable { get; init; }
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}
