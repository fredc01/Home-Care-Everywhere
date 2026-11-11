using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class MaintenanceCompletion
{
    public int Id { get; set; }
    public int MaintenanceTaskId { get; set; }
    public MaintenanceTask MaintenanceTask { get; set; } = null!;
    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;

    [MaxLength(1200)] public string? Notes { get; set; }
    [Required] public string CompletedByUserId { get; set; } = string.Empty;
    public ApplicationUser CompletedByUser { get; set; } = null!;
    public ICollection<MaintenanceExpense> Expenses { get; set; } = new List<MaintenanceExpense>();
}
