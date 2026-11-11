using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class MaintenanceExpense
{
    public int Id { get; set; }
    public int MaintenanceCompletionId { get; set; }
    public MaintenanceCompletion MaintenanceCompletion { get; set; } = null!;

    [Range(0, 999999999)] public decimal Amount { get; set; }
    [MaxLength(160)] public string? Vendor { get; set; }
    [MaxLength(300)] public string? Description { get; set; }
    public DateTime ExpenseDateUtc { get; set; } = DateTime.UtcNow;
}
