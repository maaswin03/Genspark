using System.Security.Claims;
using backend.Models.DTOs.Notification;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
	private readonly INotificationService _service;

	public NotificationController(INotificationService service)
	{
		_service = service;
	}

	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<NotificationResponse>>> GetForUserAsync(
		[FromQuery] bool unread = false,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return Ok(await _service.GetForUserAsync(GetUserId(), unread, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("{id:int}/read")]
	public async Task<ActionResult> MarkReadAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			await _service.MarkReadAsync(GetUserId(), id, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("read-all")]
	public async Task<ActionResult> MarkAllReadAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _service.MarkAllReadAsync(GetUserId(), cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	private int GetUserId()
	{
		var claim = User.FindFirstValue("userId");
		if (!int.TryParse(claim, out var userId))
		{
			throw new UnauthorizedAccessException("Invalid token.");
		}

		return userId;
	}

	private ActionResult HandleException(Exception ex)
	{
		return ex switch
		{
			UnauthorizedAccessException => Unauthorized(ex.Message),
			KeyNotFoundException => NotFound(ex.Message),
			ArgumentException => BadRequest(ex.Message),
			InvalidOperationException => BadRequest(ex.Message),
			_ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
		};
	}
}
