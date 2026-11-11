using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public enum CareVisitStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}

public class CareVisit
{
    public int Id { get; set; }

    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;

    public int CaregiverId { get; set; }
    public Caregiver Caregiver { get; set; } = null!;

    public DateTime ScheduledAtUtc { get; set; }

    [Range(15, 1440)]
    public int ExpectedDurationMinutes { get; set; } = 60;

    public CareVisitStatus Status { get; set; } = CareVisitStatus.Scheduled;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(1200)] public string? Instructions { get; set; }
    [MaxLength(2000)] public string? CompletionNotes { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
