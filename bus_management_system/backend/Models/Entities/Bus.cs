using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("buses")]
public class Bus
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("operator_id")]
    public int OperatorId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("bus_number")]
    public required string BusNumber { get; set; }

    [Column("bus_type")]
    public BusType BusType { get; set; }

    [Column("total_seats")]
    public int TotalSeats { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(OperatorId))]
    public Operator Operator { get; set; } = null!;

    public ICollection<Seat> Seats { get; set; } = new HashSet<Seat>();
    public ICollection<Trip> Trips { get; set; } = new HashSet<Trip>();
}
