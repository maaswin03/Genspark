using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Booking;

public class ReserveSeatRequest
{
    [Required]
    public int TripId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one seat must be selected.")]
    public required List<int> SeatIds { get; set; }
}

public class ReserveSeatResponse
{
    public int TripId { get; set; }
    public DateTime LockedUntil { get; set; }
    public List<int> ReservedTripSeatIds { get; set; } = new();
}

public class CreateBookingRequest
{
    [Required]
    public int TripId { get; set; }

    [Required]
    public int BoardingPointId { get; set; }

    [Required]
    public int DroppingPointId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one passenger is required.")]
    public required List<BookingPassengerRequest> Passengers { get; set; }
}

public class BookingPassengerRequest
{
    [Required]
    public int TripSeatId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }

    [Required]
    [RegularExpression("^(male|female|other)$", ErrorMessage = "Gender must be male, female, or other.")]
    public required string Gender { get; set; }
}

public class CancelBookingRequest
{
    [Required]
    [MaxLength(500)]
    public required string CancelReason { get; set; }
}
