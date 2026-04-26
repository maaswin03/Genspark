using backend.Models.DTOs.Notification;

namespace backend.Services;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
        int userId,
        bool unread,
        CancellationToken cancellationToken = default);

    Task MarkReadAsync(
        int userId,
        int notificationId,
        CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
