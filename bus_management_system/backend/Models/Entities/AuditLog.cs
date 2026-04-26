using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("audit_logs")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("actor_id")]
    public int ActorId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("action")]
    public required string Action { get; set; }

    [Column("entity_type")]
    public EntityType EntityType { get; set; }

    [Column("entity_id")]
    public int EntityId { get; set; }

    [Column("old_value", TypeName = "jsonb")]
    public string? OldValue { get; set; }

    [Column("new_value", TypeName = "jsonb")]
    public string? NewValue { get; set; }

    [MaxLength(45)]
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(ActorId))]
    [JsonIgnore]
    public User Actor { get; set; } = null!;
}
