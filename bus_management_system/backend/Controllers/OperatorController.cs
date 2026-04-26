using System.Security.Claims;
using backend.Models.DTOs.Admin;
using backend.Models.DTOs.Operator;
using backend.Models.DTOs.Revenue;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/operator")]
[Authorize(Roles = "operator")]
public class OperatorController : ControllerBase
{
	private readonly IOperatorService _service;
	private readonly IRevenueService _revenueService;
	private readonly IRouteService _routeService;

	public OperatorController(IOperatorService service, IRevenueService revenueService, IRouteService routeService)
	{
		_service = service;
		_revenueService = revenueService;
		_routeService = routeService;
	}

	[HttpGet("routes")]
	public async Task<ActionResult<IReadOnlyList<RouteResponse>>> GetRouteOptionsAsync(CancellationToken cancellationToken)
	{
		try
		{
			var routes = await _routeService.GetAllAsync(cancellationToken);
			return Ok(routes.Where(route => route.IsActive).ToList());
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("profile")]
	public async Task<ActionResult<OperatorProfileResponse>> GetProfileAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.GetProfileAsync(userId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPut("profile")]
	public async Task<ActionResult<OperatorProfileResponse>> UpdateProfileAsync([FromBody] UpdateOperatorProfileRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.UpdateProfileAsync(userId, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPost("change-password")]
	public async Task<ActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			await _service.ChangePasswordAsync(userId, request, cancellationToken);
			return Ok(new { message = "Password updated successfully." });
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPost("documents")]
	public async Task<ActionResult<OperatorDocumentResponse>> UploadDocumentAsync([FromBody] UploadDocumentRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.UploadDocumentAsync(userId, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("documents")]
	public async Task<ActionResult<IReadOnlyList<OperatorDocumentResponse>>> GetDocumentsAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.GetDocumentsAsync(userId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("offices")]
	public async Task<ActionResult<IReadOnlyList<OperatorOfficeResponse>>> GetOfficesAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.GetOfficesAsync(userId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPost("offices")]
	public async Task<ActionResult<OperatorOfficeResponse>> AddOfficeAsync([FromBody] CreateOfficeRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.AddOfficeAsync(userId, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPut("offices/{officeId:int}")]
	public async Task<ActionResult<OperatorOfficeResponse>> UpdateOfficeAsync(int officeId, [FromBody] UpdateOfficeRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.UpdateOfficeAsync(userId, officeId, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("bookings")]
	public async Task<ActionResult<IReadOnlyList<OperatorBookingSummaryResponse>>> GetOperatorBookingsAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.GetOperatorBookingsAsync(userId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("bookings/{bookingId:int}")]
	public async Task<ActionResult<OperatorBookingDetailResponse>> GetBookingDetailAsync(int bookingId, CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _service.GetBookingDetailAsync(userId, bookingId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("revenue")]
	public async Task<ActionResult<OperatorRevenueSummaryResponse>> GetRevenueAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _revenueService.GetOperatorRevenueAsync(userId, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("revenue/trips")]
	public async Task<ActionResult<IReadOnlyList<OperatorTripRevenueResponse>>> GetTripRevenueAsync(CancellationToken cancellationToken)
	{
		try
		{
			var userId = GetUserId();
			return Ok(await _revenueService.GetOperatorTripRevenueAsync(userId, cancellationToken));
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
