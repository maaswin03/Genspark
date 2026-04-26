using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("operator_documents")]
public class OperatorDocument
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("operator_id")]
    public int OperatorId { get; set; }

    [Column("document_type")]
    public DocumentType DocumentType { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("file_url")]
    public required string FileUrl { get; set; }

    [Column("uploaded_at")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Column("verified_by")]
    public int? VerifiedBy { get; set; }

    [Column("verified_at")]
    public DateTime? VerifiedAt { get; set; }

    [ForeignKey(nameof(OperatorId))]
    [JsonIgnore]
    public Operator Operator { get; set; } = null!;

    [ForeignKey(nameof(VerifiedBy))]
    [JsonIgnore]
    public User? VerifiedByUser { get; set; }
}
