using System.Security.Claims;
using backend.Models.DTOs.Payment;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "user")]
public class PaymentController : ControllerBase
{
	private readonly IPaymentService _service;

	public PaymentController(IPaymentService service)
	{
		_service = service;
	}

	[HttpPost("initiate")]
	public async Task<ActionResult<InitiatePaymentResponse>> InitiateAsync(
		[FromBody] InitiatePaymentRequest request,
		CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.InitiateAsync(GetUserId(), request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPost("webhook")]
	public async Task<ActionResult> HandleWebhookAsync(
		[FromBody] PaymentWebhookRequest request,
		CancellationToken cancellationToken)
	{
		try
		{
			await _service.HandleWebhookAsync(GetUserId(), request, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("{bookingId:int}")]
	public async Task<ActionResult<PaymentStatusResponse>> GetStatusAsync(
		int bookingId,
		CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetStatusAsync(GetUserId(), bookingId, cancellationToken));
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
