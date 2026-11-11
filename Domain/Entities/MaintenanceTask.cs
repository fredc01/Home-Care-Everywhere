using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public enum MaintenanceFrequency
{
    OneTime,
    Weekly,
    Monthly,
    Quarterly,
    SemiAnnual,
    Annual,
    Custom
}

public class MaintenanceTask
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int? ApplianceId { get; set; }
    public Appliance? Appliance { get; set; }

    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1200)]
    public string? Description { get; set; }

    public MaintenanceFrequency Frequency { get; set; } = MaintenanceFrequency.OneTime;
    [Range(1, 3650)] public int? CustomIntervalDays { get; set; }
    public DateOnly NextDueDate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    [Required] public string CreatedByUserId { get; set; } = string.Empty;
    public ApplicationUser CreatedByUser { get; set; } = null!;

    public ICollection<MaintenanceCompletion> Completions { get; set; } = new List<MaintenanceCompletion>();
}
