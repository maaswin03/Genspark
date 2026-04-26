using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("trips")]
public class Trip
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("schedule_id")]
    public int? ScheduleId { get; set; }

    [Column("bus_id")]
    public int BusId { get; set; }

    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("operator_id")]
    public int OperatorId { get; set; }

    [Column("departure_time")]
    public DateTime DepartureTime { get; set; }

    [Column("arrival_time")]
    public DateTime ArrivalTime { get; set; }

    [Column("base_fare", TypeName = "numeric(10,2)")]
    public decimal BaseFare { get; set; }

    [Column("status")]
    public TripStatus Status { get; set; } = TripStatus.Scheduled;

    [Column("cancellation_reason")]
    public string? CancellationReason { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(BusId))]
    public Bus Bus { get; set; } = null!;

    [ForeignKey(nameof(RouteId))]
    public Route Route { get; set; } = null!;

    [ForeignKey(nameof(ScheduleId))]
    public TripSchedule? Schedule { get; set; }

    [ForeignKey(nameof(OperatorId))]
    public Operator Operator { get; set; } = null!;

    public ICollection<TripSeat> TripSeats { get; set; } = new HashSet<TripSeat>();
    public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
}
