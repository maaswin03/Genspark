using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.Operator;

public class UpdateOperatorProfileRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public required string Phone { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
    public required string CompanyName { get; set; }
}

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Current password is required.")]
    [MinLength(6, ErrorMessage = "Current password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Current password cannot exceed 100 characters.")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters.")]
    [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters.")]
    public required string NewPassword { get; set; }
}

public class UploadDocumentRequest
{
    [Required(ErrorMessage = "Document type is required.")]
    public DocumentType DocumentType { get; set; }

    [Required(ErrorMessage = "File URL is required.")]
    [MaxLength(500, ErrorMessage = "File URL cannot exceed 500 characters.")]
    public required string FileUrl { get; set; }
}

public class OperatorDocumentResponse
{
    public int Id { get; set; }
    public DocumentType DocumentType { get; set; }
    public required string FileUrl { get; set; }
    public DateTime UploadedAt { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class CreateOfficeRequest
{
    [Required(ErrorMessage = "Location id is required.")]
    public int LocationId { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public required string Address { get; set; }

    public bool IsHeadOffice { get; set; }
}

public class UpdateOfficeRequest
{
    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public required string Address { get; set; }

    public bool IsHeadOffice { get; set; }
}

public class OperatorOfficeResponse
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public required string LocationName { get; set; }
    public required string Address { get; set; }
    public bool IsHeadOffice { get; set; }
}

public class OperatorBookingSummaryResponse
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public decimal TotalAmount { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public int SeatCount { get; set; }
}

public class OperatorBookingDetailResponse
{
    public int BookingId { get; set; }
    public int TripId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string UserPhone { get; set; }
    public required string RouteName { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
    public List<OperatorBookingSeatResponse> Seats { get; set; } = new();
}

public class OperatorBookingSeatResponse
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