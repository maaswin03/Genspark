using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("trip_seats")]
public class TripSeat
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Column("trip_id")]
	public int TripId { get; set; }

	[Column("seat_id")]
	public int SeatId { get; set; }

	[Column("status")]
	public SeatStatus Status { get; set; } = SeatStatus.Available;

	[Column("locked_until")]
	public DateTime? LockedUntil { get; set; }

	[Column("locked_by")]
	public int? LockedBy { get; set; }

	[ForeignKey(nameof(TripId))]
	public Trip Trip { get; set; } = null!;

	[ForeignKey(nameof(SeatId))]
	public Seat Seat { get; set; } = null!;

	[ForeignKey(nameof(LockedBy))]
	public User? LockedByUser { get; set; }

	public ICollection<BookingSeat> BookingSeats { get; set; } = new HashSet<BookingSeat>();
}
