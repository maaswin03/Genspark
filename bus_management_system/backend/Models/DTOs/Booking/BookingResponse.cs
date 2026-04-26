using backend.Models.Enums;

namespace backend.Models.DTOs.Booking;

public class BookingResponse
{
    public int BookingId { get; set; }
    public required string TripDetails { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public decimal PlatformFee { get; set; } 
    public DateTime BookingDate { get; set; }
}

public class BookingDetailResponse
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal PlatformFee { get; set; } 
    public DateTime BookingDate { get; set; }
    public List<BookingSeatDetail> Seats { get; set; } = new();
}

public class BookingSeatDetail
{
    public int BookingSeatId { get; set; }
    public int TripSeatId { get; set; }
    public int SeatId { get; set; }
    public required string SeatNumber { get; set; }
    public decimal AmountPaid { get; set; }
    public BookingSeatStatus Status { get; set; }
    public required string PassengerName { get; set; }
    public int PassengerAge { get; set; }
    public required string PassengerGender { get; set; }
}

public class CancelSeatResponse
{
    public int BookingId { get; set; }
    public int BookingSeatId { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public decimal UpdatedTotalAmount { get; set; }
}
