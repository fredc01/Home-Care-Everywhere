using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public enum RepairRequestPriority { Low, Normal, High, Urgent }
public enum RepairRequestStatus { Open, Quoted, Booked, InProgress, Completed, Cancelled }
public enum QuotationStatus { Submitted, Accepted, Declined, Expired }
public enum BookingStatus { Requested, Confirmed, Completed, Cancelled }

public class RepairRequest
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int? ApplianceId { get; set; }
    public Appliance? Appliance { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = string.Empty;
    [Required, MaxLength(2500)] public string Description { get; set; } = string.Empty;
    public RepairRequestPriority Priority { get; set; } = RepairRequestPriority.Normal;
    public RepairRequestStatus Status { get; set; } = RepairRequestStatus.Open;
    public DateOnly? PreferredDate { get; set; }
    [Required] public string RequestedByUserId { get; set; } = string.Empty;
    public ApplicationUser RequestedByUser { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<RepairFeedback> Feedback { get; set; } = new List<RepairFeedback>();
}

public class Quotation
{
    public int Id { get; set; }
    public int RepairRequestId { get; set; }
    public RepairRequest RepairRequest { get; set; } = null!;
    public int ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;
    [Range(0, 999999999)] public decimal Amount { get; set; }
    [Required, MaxLength(1200)] public string Description { get; set; } = string.Empty;
    public DateTime ValidUntilUtc { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Submitted;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Booking
{
    public int Id { get; set; }
    public int RepairRequestId { get; set; }
    public RepairRequest RepairRequest { get; set; } = null!;
    public int ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;
    public int? QuotationId { get; set; }
    public Quotation? Quotation { get; set; }
    public DateTime ScheduledStartUtc { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Requested;
    [MaxLength(1200)] public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class RepairFeedback
{
    public int Id { get; set; }
    public int RepairRequestId { get; set; }
    public RepairRequest RepairRequest { get; set; } = null!;
    public int ServiceProviderId { get; set; }
    public ServiceProvider ServiceProvider { get; set; } = null!;
    [Required] public string SubmittedByUserId { get; set; } = string.Empty;
    public ApplicationUser SubmittedByUser { get; set; } = null!;
    [Range(1, 5)] public int Rating { get; set; }
    [MaxLength(1200)] public string? Comments { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
