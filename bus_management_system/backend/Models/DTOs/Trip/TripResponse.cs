using backend.Models.Enums;

namespace backend.Models.DTOs.Trip;

public class TripResponse
{
    public int TripId { get; set; }
    public required string OperatorName { get; set; }
    public required string BusNumber { get; set; }
    public BusType BusType { get; set; }
    public required string RouteName { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal BaseFare { get; set; }
    public int AvailableSeats { get; set; }
    public TripStatus Status { get; set; }
}

public class TripDetailResponse : TripResponse
{
    public int RouteId { get; set; }
    public required string SourceName { get; set; }
    public required string DestinationName { get; set; }
    public List<TripSeatResponse> Seats { get; set; } = new();
}

public class TripPointResponse
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public required string LocationName { get; set; }
    public required string Name { get; set; }
    public int TimeOffset { get; set; }
    public bool IsDefault { get; set; }
}

public class TripPointsResponse
{
    public List<TripPointResponse> BoardingPoints { get; set; } = new();
    public List<TripPointResponse> DroppingPoints { get; set; } = new();
}
