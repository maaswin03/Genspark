using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("notifications")]
public class Notification
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("title")]
    public required string Title { get; set; }

    [Required]
    [Column("message")]
    public required string Message { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("type")]
    public required string Type { get; set; }

    [Column("reference_type")]
    public ReferenceType? ReferenceType { get; set; }

    [Column("reference_id")]
    public int? ReferenceId { get; set; }

    [Column("channel")]
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    [Column("is_read")]
    public bool IsRead { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    [JsonIgnore]
    public User User { get; set; } = null!;
}
