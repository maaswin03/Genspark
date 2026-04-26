using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models.Entities;

[Table("route_stops")]
public class RouteStop
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("stop_order")]
    public int StopOrder { get; set; }

    [Column("distance_from_source")]
    public int DistanceFromSource { get; set; }

    [ForeignKey(nameof(RouteId))]
    [JsonIgnore]
    public Route Route { get; set; } = null!;

    [ForeignKey(nameof(LocationId))]
    [JsonIgnore]
    public Location Location { get; set; } = null!;
}
