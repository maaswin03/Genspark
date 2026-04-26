using System.Security.Claims;
using backend.Models.DTOs.Booking;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize(Roles = "user")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingController(IBookingService service)
    {
        _service = service;
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<ReserveSeatResponse>> ReserveSeatAsync(
        [FromBody] ReserveSeatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.ReserveSeatAsync(GetUserId(), request, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> CreateBookingAsync(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Temporarily add this at the top of your CreateBooking method
Console.WriteLine($"TripId: {request.TripId}, BoardingPointId: {request.BoardingPointId}, DroppingPointId: {request.DroppingPointId}");
            return Ok(await _service.CreateBookingAsync(GetUserId(), request, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{bookingId:int}")]
    public async Task<ActionResult<BookingDetailResponse>> GetBookingDetailAsync(
        int bookingId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetBookingDetailAsync(GetUserId(), bookingId, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPatch("{bookingId:int}/cancel")]
    public async Task<ActionResult<BookingResponse>> CancelBookingAsync(
        int bookingId,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.CancelBookingAsync(GetUserId(), bookingId, request.CancelReason, cancellationToken));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPatch("{bookingId:int}/seats/{bookingSeatId:int}/cancel")]
    public async Task<ActionResult<CancelSeatResponse>> CancelSeatAsync(
        int bookingId,
        int bookingSeatId,
        [FromBody] CancelBookingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.CancelSeatAsync(GetUserId(), bookingId, bookingSeatId, request.CancelReason, cancellationToken));
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
