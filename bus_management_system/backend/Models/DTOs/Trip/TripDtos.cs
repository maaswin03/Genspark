using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.Trip;

public class CreateScheduleRequest
{
    [Required]
    public int BusId { get; set; }

    [Required]
    public int RouteId { get; set; }

    [Required]
    public TimeSpan DepartureTime { get; set; }

    [Required]
    public TimeSpan ArrivalTime { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal BaseFare { get; set; }

    [Required]
    [MaxLength(64)]
    public required string DaysOfWeek { get; set; }

    [Required]
    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidUntil { get; set; }
}

public class UpdateScheduleRequest : CreateScheduleRequest
{
    public bool IsActive { get; set; }
}

public class TripScheduleResponse
{
    public int Id { get; set; }
    public int BusId { get; set; }
    public int RouteId { get; set; }
    public int OperatorId { get; set; }
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal BaseFare { get; set; }
    public required string DaysOfWeek { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public bool IsActive { get; set; }
}

public class ChangeBusRequest
{
    [Required]
    public int NewBusId { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Reason { get; set; }
}

public class CancelTripRequest
{
    [Required]
    [MaxLength(500)]
    public required string Reason { get; set; }
}

public class SeatPricingRequest
{
    [Required]
    public int SeatId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}

public class SetSeatPricingRequest
{
    [Required]
    [MinLength(1)]
    public required List<SeatPricingRequest> Prices { get; set; }
}

public class SeatPricingResponse
{
    public int SeatId { get; set; }
    public required string SeatNumber { get; set; }
    public decimal Price { get; set; }
}

public class TripSeatResponse
{
    public int TripSeatId { get; set; }
    public int SeatId { get; set; }
    public required string SeatNumber { get; set; }
    public int Row { get; set; }
    public int ColumnNumber { get; set; }
    public DeckType Deck { get; set; }
    public SeatType SeatType { get; set; }
    public SeatStatus Status { get; set; }
    public DateTime? LockedUntil { get; set; }
    public decimal Price { get; set; }
}

public class OperatorTripResponse
{
    public int TripId { get; set; }
    public int ScheduleId { get; set; }
    public required string BusNumber { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal BaseFare { get; set; }
    public TripStatus Status { get; set; }
    public int AvailableSeats { get; set; }
    public int BookedSeats { get; set; }
}
