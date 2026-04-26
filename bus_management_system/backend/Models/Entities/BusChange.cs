using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("bus_changes")]
public class BusChange
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("trip_id")]
    public int TripId { get; set; }

    [Column("old_bus_id")]
    public int OldBusId { get; set; }

    [Column("new_bus_id")]
    public int NewBusId { get; set; }

    [Column("change_type")]
    public ChangeType ChangeType { get; set; }

    [Required]
    [Column("reason")]
    public required string Reason { get; set; }

    [Column("changed_by")]
    public int ChangedBy { get; set; }

    [Column("changed_at")]
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Column("reverted_at")]
    public DateTime? RevertedAt { get; set; }

    [ForeignKey(nameof(TripId))]
    [JsonIgnore]
    public Trip Trip { get; set; } = null!;

    [ForeignKey(nameof(OldBusId))]
    [JsonIgnore]
    public Bus OldBus { get; set; } = null!;

    [ForeignKey(nameof(NewBusId))]
    [JsonIgnore]
    public Bus NewBus { get; set; } = null!;

    [ForeignKey(nameof(ChangedBy))]
    [JsonIgnore]
    public User ChangedByUser { get; set; } = null!;
}
