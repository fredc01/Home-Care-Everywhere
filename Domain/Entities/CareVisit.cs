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

    public CareVisitStatus Status { get; set; } = CareVisitStatus.Scheduled;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
