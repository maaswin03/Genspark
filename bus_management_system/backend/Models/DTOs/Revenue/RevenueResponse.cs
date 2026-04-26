namespace backend.Models.DTOs.Revenue;

public class AdminRevenueSummaryResponse
{
	public decimal TotalRevenue { get; set; }
	public decimal TotalPlatformFee { get; set; }
	public decimal TotalOperatorEarning { get; set; }
	public int TotalTransactions { get; set; }
}

public class AdminOperatorRevenueResponse
{
	public int OperatorId { get; set; }
	public required string CompanyName { get; set; }
	public decimal TotalRevenue { get; set; }
	public decimal TotalPlatformFee { get; set; }
	public decimal TotalOperatorEarning { get; set; }
	public int TotalTransactions { get; set; }
}

public class OperatorRevenueSummaryResponse
{
	public int OperatorId { get; set; }
	public required string CompanyName { get; set; }
	public decimal TotalRevenue { get; set; }
	public decimal TotalPlatformFee { get; set; }
	public decimal TotalOperatorEarning { get; set; }
	public int TotalTransactions { get; set; }
}

public class OperatorTripRevenueResponse
{
	public int TripId { get; set; }
	public DateTime DepartureTime { get; set; }
	public DateTime ArrivalTime { get; set; }
	public required string RouteName { get; set; }
	public decimal TotalRevenue { get; set; }
	public decimal TotalPlatformFee { get; set; }
	public decimal OperatorEarning { get; set; }
	public int TotalBookings { get; set; }
}
