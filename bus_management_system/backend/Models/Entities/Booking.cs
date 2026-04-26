using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("bookings")]
public class Booking
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("user_id")]
	public int UserId { get; set; }

	[Column("trip_id")]
	public int TripId { get; set; }

	[Column("boarding_point_id")]
	public int BoardingPointId { get; set; }

	[Column("dropping_point_id")]
	public int DroppingPointId { get; set; }

	[Column("booking_date")]
	public DateTime BookingDate { get; set; } = DateTime.UtcNow;

	[Column("total_amount", TypeName = "numeric(10,2)")]
	public decimal TotalAmount { get; set; }

	[Column("platform_fee", TypeName = "numeric(10,2)")]
	public decimal PlatformFee { get; set; }

	[Column("status")]
	public BookingStatus Status { get; set; } = BookingStatus.Pending;

	[Column("cancelled_at")]
	public DateTime? CancelledAt { get; set; }

	[Column("cancel_reason")]
	public string? CancelReason { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	[ForeignKey(nameof(UserId))]
	public User User { get; set; } = null!;

	[ForeignKey(nameof(TripId))]
	public Trip Trip { get; set; } = null!;

	[ForeignKey(nameof(BoardingPointId))]
	public BoardingPoint BoardingPoint { get; set; } = null!;

	[ForeignKey(nameof(DroppingPointId))]
	public DroppingPoint DroppingPoint { get; set; } = null!;

	public ICollection<BookingSeat> BookingSeats { get; set; } = new HashSet<BookingSeat>();
	public ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
	public Transaction? Transaction { get; set; }
}
