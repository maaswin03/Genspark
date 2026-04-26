using System.Security.Claims;
using backend.Jobs;
using backend.Models.DTOs.Trip;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class TripController : ControllerBase
{
	private readonly ITripService _service;
	private readonly TripGeneratorJob _tripGeneratorJob;

	public TripController(ITripService service, TripGeneratorJob tripGeneratorJob)
	{
		_service = service;
		_tripGeneratorJob = tripGeneratorJob;
	}

	[HttpGet("trips/search")]
	public async Task<ActionResult<IReadOnlyList<TripResponse>>> SearchAsync([FromQuery] TripSearchRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.SearchAsync(request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("trips/{id:int}")]
	public async Task<ActionResult<TripDetailResponse>> GetDetailAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetDetailAsync(id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("trips/{id:int}/seats")]
	public async Task<ActionResult<IReadOnlyList<TripSeatResponse>>> GetAvailableSeatsAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetAvailableSeatsAsync(id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("trips/{id:int}/points")]
	public async Task<ActionResult<TripPointsResponse>> GetTripPointsAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetPointsAsync(id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpGet("operator/trips")]
	public async Task<ActionResult<IReadOnlyList<OperatorTripResponse>>> GetByOperatorAsync(CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetByOperatorAsync(GetUserId(), cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpGet("operator/trips/schedules")]
	public async Task<ActionResult<IReadOnlyList<TripScheduleResponse>>> GetSchedulesByOperatorAsync(CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetSchedulesByOperatorAsync(GetUserId(), cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPost("operator/trips/schedules")]
	public async Task<ActionResult<TripScheduleResponse>> CreateScheduleAsync([FromBody] CreateScheduleRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.CreateScheduleAsync(GetUserId(), request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPut("operator/trips/schedules/{id:int}")]
	public async Task<ActionResult<TripScheduleResponse>> UpdateScheduleAsync(int id, [FromBody] UpdateScheduleRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.UpdateScheduleAsync(GetUserId(), id, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPatch("operator/trips/schedules/{id:int}/toggle")]
	public async Task<ActionResult<TripScheduleResponse>> ToggleScheduleAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.ToggleScheduleAsync(GetUserId(), id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator,admin")]
	[HttpPost("operator/trips/generate-today")]
	public async Task<ActionResult> GenerateTodayTripsAsync(CancellationToken cancellationToken)
	{
		try
		{
			await _tripGeneratorJob.ProcessSchedulesAsync(cancellationToken);
			return Ok("Trip generation completed for today.");
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPost("operator/trips/{id:int}/change-bus")]
	public async Task<ActionResult<TripDetailResponse>> ChangeBusAsync(int id, [FromBody] ChangeBusRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.ChangeBusAsync(GetUserId(), id, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPatch("operator/trips/{id:int}/cancel")]
	public async Task<ActionResult<TripDetailResponse>> CancelAsync(int id, [FromBody] CancelTripRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.CancelAsync(GetUserId(), id, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpGet("operator/trips/{id:int}/pricing")]
	public async Task<ActionResult<IReadOnlyList<SeatPricingResponse>>> GetSeatPricingAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetSeatPricingAsync(GetUserId(), id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPost("operator/trips/{id:int}/pricing")]
	public async Task<ActionResult<IReadOnlyList<SeatPricingResponse>>> SetSeatPricingAsync(int id, [FromBody] SetSeatPricingRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.SetSeatPricingAsync(GetUserId(), id, request, cancellationToken));
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
