using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace backend.Models.Entities;

[Table("operator_offices")]
public class OperatorOffice
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("operator_id")]
    public int OperatorId { get; set; }

    [Column("location_id")]
    public int LocationId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("address")]
    public required string Address { get; set; }

    [Column("is_head_office")]
    public bool IsHeadOffice { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(OperatorId))]
    [JsonIgnore]
    public Operator Operator { get; set; } = null!;

    [ForeignKey(nameof(LocationId))]
    [JsonIgnore]
    public Location Location { get; set; } = null!;
}
