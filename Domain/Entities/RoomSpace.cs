using System.ComponentModel.DataAnnotations;

namespace HomeCarePH.Domain.Entities;

public class RoomSpace
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? SpaceType { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
