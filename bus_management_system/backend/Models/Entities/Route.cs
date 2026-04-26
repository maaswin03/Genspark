using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models.Entities;

[Table("routes")]
public class Route
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("source_id")]
    public int SourceId { get; set; }

    [Column("destination_id")]
    public int DestinationId { get; set; }

    [Column("distance_km")]
    public int DistanceKm { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(SourceId))]
    [JsonIgnore]
    public Location SourceLocation { get; set; } = null!;

    [ForeignKey(nameof(DestinationId))]
    [JsonIgnore]
    public Location DestinationLocation { get; set; } = null!;

    [ForeignKey(nameof(CreatedBy))]
    [JsonIgnore]
    public User CreatedByUser { get; set; } = null!;
}
