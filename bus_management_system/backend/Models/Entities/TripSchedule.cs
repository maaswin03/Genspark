using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models.Entities;

[Table("trip_schedules")]
public class TripSchedule
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bus_id")]
    public int BusId { get; set; }

    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("operator_id")]
    public int OperatorId { get; set; }

    [Column("departure_time")]
    public TimeSpan DepartureTime { get; set; }

    [Column("arrival_time")]
    public TimeSpan ArrivalTime { get; set; }

    [Column("base_fare", TypeName = "numeric(10,2)")]
    public decimal BaseFare { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("days_of_week")]
    public required string DaysOfWeek { get; set; }

    [Column("valid_from")]
    public DateOnly ValidFrom { get; set; }

    [Column("valid_until")]
    public DateOnly? ValidUntil { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(BusId))]
    [JsonIgnore]
    public Bus Bus { get; set; } = null!;

    [ForeignKey(nameof(RouteId))]
    [JsonIgnore]
    public Route Route { get; set; } = null!;

    [ForeignKey(nameof(OperatorId))]
    [JsonIgnore]
    public Operator Operator { get; set; } = null!;
}
