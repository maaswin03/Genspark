using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("booking_seats")]
public class BookingSeat
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("booking_id")]
    public int BookingId { get; set; }

    [Column("trip_seat_id")]
    public int TripSeatId { get; set; }

    [Column("seat_id")]
    public int SeatId { get; set; }

    [Column("amount_paid", TypeName = "numeric(10,2)")]
    public decimal AmountPaid { get; set; }

    [Column("status")]
    public BookingSeatStatus Status { get; set; } = BookingSeatStatus.Confirmed;

    [Column("cancelled_at")]
    public DateTime? CancelledAt { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking Booking { get; set; } = null!;

    [ForeignKey(nameof(TripSeatId))]
    public TripSeat TripSeat { get; set; } = null!;

    [ForeignKey(nameof(SeatId))]
    public Seat Seat { get; set; } = null!;

    public Passenger? Passenger { get; set; }
}
