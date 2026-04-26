using backend.Data;
using backend.Models.DTOs.Revenue;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class RevenueService : IRevenueService
{
	private readonly AppDbContext _context;

	public RevenueService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<AdminRevenueSummaryResponse> GetAdminRevenueAsync(CancellationToken cancellationToken = default)
	{
		var txQuery = _context.Transactions
			.AsNoTracking()
			.Include(x => x.Booking)
			.Where(x => x.TotalAmount > 0);

		return new AdminRevenueSummaryResponse
		{
			TotalRevenue = await txQuery.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0,
			TotalPlatformFee = await txQuery.SumAsync(x => (decimal?)x.PlatformFee, cancellationToken) ?? 0,
			TotalOperatorEarning = await txQuery.SumAsync(x => (decimal?)x.OperatorEarning, cancellationToken) ?? 0,
			TotalTransactions = await txQuery.CountAsync(cancellationToken)
		};
	}

	public async Task<IReadOnlyList<AdminOperatorRevenueResponse>> GetAdminOperatorRevenueAsync(CancellationToken cancellationToken = default)
	{
		var rows = await _context.Transactions
			.AsNoTracking()
			.Include(x => x.Operator)
			.Include(x => x.Booking)
			.Where(x => x.TotalAmount > 0)
			.GroupBy(x => new { x.OperatorId, x.Operator.CompanyName })
			.Select(g => new AdminOperatorRevenueResponse
			{
				OperatorId = g.Key.OperatorId,
				CompanyName = g.Key.CompanyName,
				TotalRevenue = g.Sum(x => x.TotalAmount),
				TotalPlatformFee = g.Sum(x => x.PlatformFee),
				TotalOperatorEarning = g.Sum(x => x.OperatorEarning),
				TotalTransactions = g.Count()
			})
			.OrderByDescending(x => x.TotalRevenue)
			.ToListAsync(cancellationToken);

		return rows;
	}

	public async Task<OperatorRevenueSummaryResponse> GetOperatorRevenueAsync(int userId, CancellationToken cancellationToken = default)
	{
		var op = await _context.Operators
			.AsNoTracking()
			.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

		if (op == null)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		var txQuery = _context.Transactions
			.AsNoTracking()
			.Include(x => x.Booking)
			.Where(x => x.OperatorId == op.Id && x.TotalAmount > 0);

		return new OperatorRevenueSummaryResponse
		{
			OperatorId = op.Id,
			CompanyName = op.CompanyName,
			TotalRevenue = await txQuery.SumAsync(x => (decimal?)x.TotalAmount, cancellationToken) ?? 0,
			TotalPlatformFee = await txQuery.SumAsync(x => (decimal?)x.PlatformFee, cancellationToken) ?? 0,
			TotalOperatorEarning = await txQuery.SumAsync(x => (decimal?)x.OperatorEarning, cancellationToken) ?? 0,
			TotalTransactions = await txQuery.CountAsync(cancellationToken)
		};
	}

	public async Task<IReadOnlyList<OperatorTripRevenueResponse>> GetOperatorTripRevenueAsync(int userId, CancellationToken cancellationToken = default)
	{
		var opId = await _context.Operators
			.AsNoTracking()
			.Where(x => x.UserId == userId)
			.Select(x => x.Id)
			.FirstOrDefaultAsync(cancellationToken);

		if (opId == 0)
		{
			throw new KeyNotFoundException("Operator profile not found.");
		}

		var rows = await _context.Transactions
			.AsNoTracking()
			.Include(x => x.Booking)
				.ThenInclude(x => x.Trip)
					.ThenInclude(x => x.Route)
						.ThenInclude(x => x.SourceLocation)
			.Include(x => x.Booking)
				.ThenInclude(x => x.Trip)
					.ThenInclude(x => x.Route)
						.ThenInclude(x => x.DestinationLocation)
			.Where(x => x.OperatorId == opId && x.TotalAmount > 0)
			.GroupBy(x => new
			{
				TripId = x.Booking.TripId,
				x.Booking.Trip.DepartureTime,
				x.Booking.Trip.ArrivalTime,
				Source = x.Booking.Trip.Route.SourceLocation.Name,
				Destination = x.Booking.Trip.Route.DestinationLocation.Name
			})
			.Select(g => new OperatorTripRevenueResponse
			{
				TripId = g.Key.TripId,
				DepartureTime = g.Key.DepartureTime,
				ArrivalTime = g.Key.ArrivalTime,
				RouteName = g.Key.Source + " -> " + g.Key.Destination,
				TotalRevenue = g.Sum(x => x.TotalAmount),
				TotalPlatformFee = g.Sum(x => x.PlatformFee),
				OperatorEarning = g.Sum(x => x.OperatorEarning),
				TotalBookings = g.Count()
			})
			.OrderByDescending(x => x.DepartureTime)
			.ToListAsync(cancellationToken);

		return rows;
	}
}
