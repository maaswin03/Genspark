using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.User;

public class UserProfileResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Role { get; set; }
}

public class UpdateUserProfileRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public required string Phone { get; set; }
}

public class UserBookingResponse
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
    public int SeatCount { get; set; }
}

public class UserBookingDetailResponse
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public List<UserBookingSeatResponse> Seats { get; set; } = new();
}

public class UserBookingSeatResponse
{
    public int BookingSeatId { get; set; }
    public int SeatId { get; set; }
    public required string SeatNumber { get; set; }
    public decimal AmountPaid { get; set; }
    public BookingSeatStatus Status { get; set; }
    public string? PassengerName { get; set; }
    public int? PassengerAge { get; set; }
    public Gender? PassengerGender { get; set; }
}

public class UserNotificationResponse
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public required string Type { get; set; }
    public ReferenceType? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public NotificationChannel Channel { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
