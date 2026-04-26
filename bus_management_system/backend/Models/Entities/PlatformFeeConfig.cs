using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("platform_fee_config")]
public class PlatformFeeConfig
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("fee_type")]
    public FeeType Type { get; set; } = FeeType.Percentage;

    [Column("fee_value", TypeName = "numeric(5,2)")]
    public decimal FeeValue { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_by")]
    public int CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedBy))]
    [JsonIgnore]
    public User CreatedByUser { get; set; } = null!;
}
