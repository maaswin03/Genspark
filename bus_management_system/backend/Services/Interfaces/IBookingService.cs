using backend.Models.DTOs.Booking;

namespace backend.Services;

public interface IBookingService
{
    Task<ReserveSeatResponse> ReserveSeatAsync(
        int userId,
        ReserveSeatRequest request,
        CancellationToken cancellationToken = default);

    Task<BookingResponse> CreateBookingAsync(
        int userId,
        CreateBookingRequest request,
        CancellationToken cancellationToken = default);

    Task<BookingDetailResponse> GetBookingDetailAsync(
        int userId,
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<BookingResponse> CancelBookingAsync(
        int userId,
        int bookingId,
        string cancelReason,
        CancellationToken cancellationToken = default);

    Task<CancelSeatResponse> CancelSeatAsync(
        int userId,
        int bookingId,
        int bookingSeatId,
        string cancelReason,
        CancellationToken cancellationToken = default);
}
