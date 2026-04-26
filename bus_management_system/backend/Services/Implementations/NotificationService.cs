using backend.Data;
using backend.Models.DTOs.Notification;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NotificationService : INotificationService
{
	private readonly AppDbContext _context;

	public NotificationService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IReadOnlyList<NotificationResponse>> GetForUserAsync(
		int userId,
		bool unread,
		CancellationToken cancellationToken = default)
	{
		var query = _context.Notifications
			.AsNoTracking()
			.Where(x => x.UserId == userId);

		if (unread)
		{
			query = query.Where(x => !x.IsRead);
		}

		return await query
			.OrderByDescending(x => x.CreatedAt)
			.Select(x => new NotificationResponse
			{
				Id = x.Id,
				Title = x.Title,
				Message = x.Message,
				Type = x.Type,
				ReferenceType = x.ReferenceType,
				ReferenceId = x.ReferenceId,
				Channel = x.Channel,
				IsRead = x.IsRead,
				CreatedAt = x.CreatedAt
			})
			.ToListAsync(cancellationToken);
	}

	public async Task MarkReadAsync(
		int userId,
		int notificationId,
		CancellationToken cancellationToken = default)
	{
		var notification = await _context.Notifications
			.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);

		if (notification == null)
		{
			throw new KeyNotFoundException("Notification not found.");
		}

		if (notification.IsRead)
		{
			return;
		}

		notification.IsRead = true;
		await _context.SaveChangesAsync(cancellationToken);
	}

	public async Task MarkAllReadAsync(
		int userId,
		CancellationToken cancellationToken = default)
	{
		var unread = await _context.Notifications
			.Where(x => x.UserId == userId && !x.IsRead)
			.ToListAsync(cancellationToken);

		if (unread.Count == 0)
		{
			return;
		}

		foreach (var notification in unread)
		{
			notification.IsRead = true;
		}

		await _context.SaveChangesAsync(cancellationToken);
	}
}
