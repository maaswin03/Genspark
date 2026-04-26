using System.Security.Claims;
using backend.Models.DTOs.Bus;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public class BusController : ControllerBase
{
	private readonly IBusService _service;

	public BusController(IBusService service)
	{
		_service = service;
	}

	[HttpGet("buses")]
	public async Task<ActionResult<IReadOnlyList<BusResponse>>> GetAllAsync(CancellationToken cancellationToken)
	{
		return Ok(await _service.GetAllAsync(cancellationToken));
	}

	[HttpGet("buses/{id:int}")]
	public async Task<ActionResult<BusDetailResponse>> GetWithLayoutAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetWithLayoutAsync(id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[HttpGet("buses/{id:int}/seats")]
	public async Task<ActionResult<IReadOnlyList<BusSeatResponse>>> GetSeatsAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.GetSeatsAsync(id, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpGet("operator/buses")]
	public async Task<ActionResult<IReadOnlyList<BusResponse>>> GetByOperatorAsync(CancellationToken cancellationToken)
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
	[HttpPost("operator/buses")]
	public async Task<ActionResult<BusDetailResponse>> CreateAsync([FromBody] CreateBusRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.CreateAsync(GetUserId(), request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPut("operator/buses/{id:int}")]
	public async Task<ActionResult<BusDetailResponse>> UpdateAsync(int id, [FromBody] UpdateBusRequest request, CancellationToken cancellationToken)
	{
		try
		{
			return Ok(await _service.UpdateAsync(GetUserId(), id, request, cancellationToken));
		}
		catch (Exception ex)
		{
			return HandleException(ex);
		}
	}

	[Authorize(Roles = "operator")]
	[HttpPatch("operator/buses/{id:int}/deactivate")]
	public async Task<ActionResult> DeactivateAsync(int id, CancellationToken cancellationToken)
	{
		try
		{
			await _service.DeactivateAsync(GetUserId(), id, cancellationToken);
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
