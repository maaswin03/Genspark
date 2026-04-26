using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.Trip;

public class TripSearchRequest
{
    [Required(ErrorMessage = "Source id is required.")]
    public int SourceId { get; set; }

    [Required(ErrorMessage = "Destination id is required.")]
    public int DestinationId { get; set; }

    [Required(ErrorMessage = "Travel date is required.")]
    public DateTime TravelDate { get; set; }

    public BusType? BusType { get; set; }
}
