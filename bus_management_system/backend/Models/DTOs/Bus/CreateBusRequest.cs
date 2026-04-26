using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;

namespace backend.Models.DTOs.Bus;

public class SeatLayoutRequest
{
	[Required(ErrorMessage = "Seat number is required.")]
	[MaxLength(20, ErrorMessage = "Seat number cannot exceed 20 characters.")]
	public required string SeatNumber { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "Row must be greater than zero.")]
	public int Row { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "Column must be greater than zero.")]
	public int ColumnNumber { get; set; }

	[Required(ErrorMessage = "Deck is required.")]
	public DeckType Deck { get; set; }

	[Required(ErrorMessage = "Seat type is required.")]
	public SeatType SeatType { get; set; }

	public bool IsActive { get; set; } = true;
}

public class CreateBusRequest
{
	[Required(ErrorMessage = "Bus number is required.")]
	[MaxLength(50, ErrorMessage = "Bus number cannot exceed 50 characters.")]
	[RegularExpression("^[A-Za-z]{2}\\s*\\d{2}\\s*[A-Za-z]{2}\\s*\\d{4}$", ErrorMessage = "Bus number format must be like TN 37 ET 4632.")]
	public required string BusNumber { get; set; }

	[Required(ErrorMessage = "Bus type is required.")]
	public BusType BusType { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "Total seats must be greater than zero.")]
	public int TotalSeats { get; set; }

	public List<SeatLayoutRequest>? Seats { get; set; }
}

public class UpdateBusRequest
{
	[Required(ErrorMessage = "Bus number is required.")]
	[MaxLength(50, ErrorMessage = "Bus number cannot exceed 50 characters.")]
	[RegularExpression("^[A-Za-z]{2}\\s*\\d{2}\\s*[A-Za-z]{2}\\s*\\d{4}$", ErrorMessage = "Bus number format must be like TN 37 ET 4632.")]
	public required string BusNumber { get; set; }

	[Required(ErrorMessage = "Bus type is required.")]
	public BusType BusType { get; set; }

	[Range(1, int.MaxValue, ErrorMessage = "Total seats must be greater than zero.")]
	public int TotalSeats { get; set; }

	public List<SeatLayoutRequest>? Seats { get; set; }
}
