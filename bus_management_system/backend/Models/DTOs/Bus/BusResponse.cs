using backend.Models.Enums;

namespace backend.Models.DTOs.Bus;

public class BusResponse
{
	public int Id { get; set; }
	public int OperatorId { get; set; }
	public required string OperatorName { get; set; }
	public required string BusNumber { get; set; }
	public BusType BusType { get; set; }
	public int TotalSeats { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
}

public class BusDetailResponse : BusResponse
{
	public List<BusSeatResponse> Seats { get; set; } = new();
}

public class BusSeatResponse
{
	public int Id { get; set; }
	public int BusId { get; set; }
	public required string SeatNumber { get; set; }
	public int Row { get; set; }
	public int ColumnNumber { get; set; }
	public DeckType Deck { get; set; }
	public SeatType SeatType { get; set; }
	public bool IsActive { get; set; }
}
