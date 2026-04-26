using backend.Models.Enums;

namespace backend.Models.DTOs.Notification;

public class NotificationResponse
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
