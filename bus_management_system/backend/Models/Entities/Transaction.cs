using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

[Table("transactions")]
public class Transaction
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("booking_id")]
	public int BookingId { get; set; }

	[Column("operator_id")]
	public int OperatorId { get; set; }

	[Column("total_amount", TypeName = "numeric(10,2)")]
	public decimal TotalAmount { get; set; }

	[Column("platform_fee", TypeName = "numeric(10,2)")]
	public decimal PlatformFee { get; set; }

	[Column("operator_earning", TypeName = "numeric(10,2)")]
	public decimal OperatorEarning { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	[ForeignKey(nameof(BookingId))]
	public Booking Booking { get; set; } = null!;

	[ForeignKey(nameof(OperatorId))]
	public Operator Operator { get; set; } = null!;
}
