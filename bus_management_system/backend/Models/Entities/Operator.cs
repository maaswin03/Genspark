using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("operators")]
public class Operator
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("user_id")]
	public int UserId { get; set; }

	[Required]
	[MaxLength(200)]
	[Column("company_name")]
	public required string CompanyName { get; set; }

	[Required]
	[MaxLength(100)]
	[Column("license_number")]
	public required string LicenseNumber { get; set; }

	[Column("approval_status")]
	public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

	[Column("approved_by")]
	public int? ApprovedBy { get; set; }

	[Column("approved_at")]
	public DateTime? ApprovedAt { get; set; }

	[Column("rejection_reason")]
	public string? RejectionReason { get; set; }

	[Column("blocked_reason")]
	public string? BlockedReason { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	[ForeignKey(nameof(UserId))]
	public User User { get; set; } = null!;

	[ForeignKey(nameof(ApprovedBy))]
	public User? ApprovedByUser { get; set; }

	public ICollection<Bus> Buses { get; set; } = new HashSet<Bus>();
	public ICollection<Trip> Trips { get; set; } = new HashSet<Trip>();
	public ICollection<Transaction> Transactions { get; set; } = new HashSet<Transaction>();
}
