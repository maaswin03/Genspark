using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Admin;

public class CreateRouteRequest
{
    [Required(ErrorMessage = "Source location id is required.")]
    public int SourceId { get; set; }

    [Required(ErrorMessage = "Destination location id is required.")]
    public int DestinationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Distance must be greater than 0.")]
    public int DistanceKm { get; set; }
}

public class AddRouteStopsRequest
{
    [Required(ErrorMessage = "Stops are required.")]
    [MinLength(1, ErrorMessage = "At least one stop is required.")]
    public required List<RouteStopInput> Stops { get; set; }
}

public class RouteStopInput
{
    [Required(ErrorMessage = "Location id is required.")]
    public int LocationId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Stop order must be at least 1.")]
    public int StopOrder { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Distance from source cannot be negative.")]
    public int DistanceFromSource { get; set; }
}

public class RouteResponse
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public required string SourceName { get; set; }
    public int DestinationId { get; set; }
    public required string DestinationName { get; set; }
    public int DistanceKm { get; set; }
    public bool IsActive { get; set; }
    public List<RouteStopResponse> Stops { get; set; } = new();
}

public class RouteStopResponse
{
    public int Id { get; set; }
    public int LocationId { get; set; }
    public required string LocationName { get; set; }
    public int StopOrder { get; set; }
    public int DistanceFromSource { get; set; }
}
