using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("seats")]
public class Seat
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bus_id")]
    public int BusId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("seat_number")]
    public required string SeatNumber { get; set; }

    [Column("row")]
    public int Row { get; set; }

    [Column("column")]
    public int ColumnNumber { get; set; }

    [Column("deck")]
    public DeckType Deck { get; set; }

    [Column("seat_type")]
    public SeatType SeatType { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(BusId))]
    public Bus Bus { get; set; } = null!;

    public ICollection<TripSeat> TripSeats { get; set; } = new HashSet<TripSeat>();
    public ICollection<BookingSeat> BookingSeats { get; set; } = new HashSet<BookingSeat>();
}
