using backend.Models.DTOs.User;

namespace backend.Services;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserBookingResponse>> GetUserBookingsAsync(int userId, string? status, CancellationToken cancellationToken = default);
    Task<UserBookingDetailResponse> GetBookingDetailAsync(int userId, int bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserNotificationResponse>> GetUserNotificationsAsync(int userId, bool unread, CancellationToken cancellationToken = default);
    Task MarkReadAsync(int userId, int id, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default);
}
