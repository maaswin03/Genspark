using System.Security.Claims;
using backend.Models.DTOs.User;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/user")]
[Authorize(Roles = "user")]
public class UserController : ControllerBase
{
	private readonly IUserService _service;

	public UserController(IUserService service)
	{
		_service = service;
	}

	[HttpGet("profile")]
	public async Task<ActionResult<UserProfileResponse>> GetProfileAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			var result = await _service.GetProfileAsync(userId, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPut("profile")]
	public async Task<ActionResult<UserProfileResponse>> UpdateProfileAsync([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			var result = await _service.UpdateProfileAsync(userId, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("bookings")]
	public async Task<ActionResult<IReadOnlyList<UserBookingResponse>>> GetUserBookingsAsync([FromQuery] string? status, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			var result = await _service.GetUserBookingsAsync(userId, status, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("bookings/{bookingId:int}")]
	public async Task<ActionResult<UserBookingDetailResponse>> GetBookingDetailAsync(int bookingId, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			var result = await _service.GetBookingDetailAsync(userId, bookingId, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("notifications")]
	public async Task<ActionResult<IReadOnlyList<UserNotificationResponse>>> GetUserNotificationsAsync([FromQuery] bool unread = false, CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = GetUserId();
			var result = await _service.GetUserNotificationsAsync(userId, unread, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("notifications/{id:int}/read")]
	public async Task<ActionResult> MarkReadAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			await _service.MarkReadAsync(userId, id, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("notifications/read-all")]
	public async Task<ActionResult> MarkAllReadAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			await _service.MarkAllReadAsync(userId, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	private int GetUserId()
	{
		var userIdClaim = User.FindFirstValue("userId");
		if (!int.TryParse(userIdClaim, out var userId))
		{
			throw new UnauthorizedAccessException("Invalid token: missing user id.");
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
