using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models.Entities;

[Table("boarding_points")]
public class BoardingPoint
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("route_id")]
    public int RouteId { get; set; }

    [Column("location_id")]
    public int LocationId { get; set; }

    [Column("office_id")]
    public int? OfficeId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("time_offset")]
    public int TimeOffset { get; set; }

    [Column("is_default")]
    public bool IsDefault { get; set; }

    [ForeignKey(nameof(RouteId))]
    [JsonIgnore]
    public Route Route { get; set; } = null!;

    [ForeignKey(nameof(LocationId))]
    [JsonIgnore]
    public Location Location { get; set; } = null!;

    [ForeignKey(nameof(OfficeId))]
    [JsonIgnore]
    public OperatorOffice? Office { get; set; }
}
