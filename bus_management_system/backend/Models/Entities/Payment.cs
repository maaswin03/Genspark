using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("payments")]
public class Payment
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("booking_id")]
	public int BookingId { get; set; }

	[Column("amount", TypeName = "numeric(10,2)")]
	public decimal Amount { get; set; }

	[Column("payment_status")]
	public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

	[Required]
	[MaxLength(50)]
	[Column("payment_method")]
	public required string PaymentMethod { get; set; }

	[MaxLength(255)]
	[Column("transaction_id")]
	public string? TransactionId { get; set; }

	[Column("gateway_response", TypeName = "jsonb")]
	public string? GatewayResponse { get; set; }

	[Column("paid_at")]
	public DateTime? PaidAt { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	[ForeignKey(nameof(BookingId))]
	public Booking Booking { get; set; } = null!;
}
