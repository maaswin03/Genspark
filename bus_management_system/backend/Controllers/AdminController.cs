using System.Security.Claims;
using backend.Models.DTOs.Admin;
using backend.Models.DTOs.Operator;
using backend.Models.DTOs.Revenue;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
	private readonly ILocationService _locationService;
	private readonly IRouteService _routeService;
	private readonly IAdminService _adminService;
	private readonly IRevenueService _revenueService;

	public AdminController(
		ILocationService locationService,
		IRouteService routeService,
		IAdminService adminService,
		IRevenueService revenueService)
	{
		_locationService = locationService;
		_routeService = routeService;
		_adminService = adminService;
		_revenueService = revenueService;
	}

	// STEP 1: LOCATION MANAGEMENT
	[HttpGet("locations")]
	public async Task<ActionResult<IReadOnlyList<LocationResponse>>> GetAllLocationsAsync(CancellationToken cancellationToken)
	{
		var result = await _locationService.GetAllAsync(cancellationToken);
		return Ok(result);
	}

	[HttpPost("locations")]
	public async Task<ActionResult<LocationResponse>> CreateLocationAsync([FromBody] CreateLocationRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			var result = await _locationService.CreateAsync(adminId, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPut("locations/{id:int}")]
	public async Task<ActionResult<LocationResponse>> UpdateLocationAsync(int id, [FromBody] UpdateLocationRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _locationService.UpdateAsync(id, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	// STEP 2: ROUTE MANAGEMENT
	[HttpGet("routes")]
	public async Task<ActionResult<IReadOnlyList<RouteResponse>>> GetAllRoutesAsync(CancellationToken cancellationToken)
	{
		var result = await _routeService.GetAllAsync(cancellationToken);
		return Ok(result);
	}

	[HttpPost("routes")]
	public async Task<ActionResult<RouteResponse>> CreateRouteAsync([FromBody] CreateRouteRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			var result = await _routeService.CreateAsync(adminId, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPost("routes/{id:int}/stops")]
	public async Task<ActionResult<RouteResponse>> AddStopsAsync(int id, [FromBody] AddRouteStopsRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _routeService.AddStopsAsync(id, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("routes/{id:int}/toggle")]
	public async Task<ActionResult<RouteResponse>> ToggleRouteActiveAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _routeService.ToggleActiveAsync(id, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	// STEP 3: OPERATOR MANAGEMENT (CORE)
	[HttpGet("operators")]
	public async Task<ActionResult<IReadOnlyList<OperatorProfileResponse>>> GetOperatorsAsync(CancellationToken cancellationToken)
	{
		var result = await _adminService.GetOperatorsAsync(cancellationToken);
		return Ok(result);
	}

	[HttpGet("operators/{id:int}")]
	public async Task<ActionResult<OperatorDetailResponse>> GetOperatorDetailAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _adminService.GetOperatorDetailAsync(id, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("operators/{id:int}/approve")]
	public async Task<ActionResult<OperatorDetailResponse>> ApproveOperatorAsync(int id, [FromBody] ApproveOperatorRequest? request, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			var result = await _adminService.ApproveOperatorAsync(id, adminId, request ?? new ApproveOperatorRequest(), cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("operators/{id:int}/reject")]
	public async Task<ActionResult<OperatorDetailResponse>> RejectOperatorAsync(int id, [FromBody] RejectOperatorRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _adminService.RejectOperatorAsync(id, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("operators/{id:int}/block")]
	public async Task<ActionResult<OperatorDetailResponse>> BlockOperatorAsync(int id, [FromBody] RejectOperatorRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var result = await _adminService.BlockOperatorAsync(id, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpPatch("operators/{id:int}/unblock")]
	public async Task<ActionResult<OperatorDetailResponse>> UnblockOperatorAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			var result = await _adminService.UnblockOperatorAsync(id, adminId, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	// STEP 4: DOCUMENT VERIFICATION (OPTIONAL)
	[HttpPatch("operators/{id:int}/documents/{docId:int}/verify")]
	public async Task<ActionResult> VerifyDocumentAsync(int id, int docId, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			await _adminService.VerifyDocumentAsync(id, docId, adminId, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	// STEP 5: USER MANAGEMENT (OPTIONAL)
	[HttpGet("users")]
	public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsersAsync(CancellationToken cancellationToken)
	{
		var result = await _adminService.GetUsersAsync(cancellationToken);
		return Ok(result);
	}

	[HttpPatch("users/{id:int}/deactivate")]
	public async Task<ActionResult> DeactivateUserAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			await _adminService.DeactivateUserAsync(id, cancellationToken);
			return NoContent();
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	// STEP 6: PLATFORM CONFIG
	[HttpGet("platform-fee")]
	public async Task<ActionResult<PlatformFeeResponse>> GetPlatformFeeAsync(CancellationToken cancellationToken)
	{
		var result = await _adminService.GetPlatformFeeAsync(cancellationToken);
		if (result == null)
		{
			return NotFound("No active platform fee found.");
		}

		return Ok(result);
	}

	[HttpPost("platform-fee")]
	public async Task<ActionResult<PlatformFeeResponse>> SetPlatformFeeAsync([FromBody] SetPlatformFeeRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var adminId = GetAdminUserId();
			var result = await _adminService.SetPlatformFeeAsync(adminId, request, cancellationToken);
			return Ok(result);
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("revenue")]
	public async Task<ActionResult<AdminRevenueSummaryResponse>> GetRevenueAsync(CancellationToken cancellationToken)
	{
		var result = await _revenueService.GetAdminRevenueAsync(cancellationToken);
		return Ok(result);
	}

	[HttpGet("revenue/operators")]
	public async Task<ActionResult<IReadOnlyList<AdminOperatorRevenueResponse>>> GetOperatorRevenueAsync(CancellationToken cancellationToken)
	{
		var result = await _revenueService.GetAdminOperatorRevenueAsync(cancellationToken);
		return Ok(result);
	}

	private int GetAdminUserId()
	{
		var claim = User.FindFirst("userId")?.Value;
		if (!int.TryParse(claim, out var userId))
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
