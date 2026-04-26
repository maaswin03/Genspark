using backend.Data;
using backend.Models.DTOs.Bus;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace backend.Services;

public class BusService : IBusService
{
    private static readonly Regex BusNumberFormat = new(@"^([A-Z]{2})\s(\d{2})\s([A-Z]{2})\s(\d{4})$", RegexOptions.Compiled);

    private readonly AppDbContext _context;

    public BusService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<BusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Buses
            .AsNoTracking()
            .Include(x => x.Operator)
            .Where(x => x.IsActive && x.DeletedAt == null)
            .OrderBy(x => x.BusNumber)
            .Select(x => new BusResponse
            {
                Id = x.Id,
                OperatorId = x.OperatorId,
                OperatorName = x.Operator.CompanyName,
                BusNumber = x.BusNumber,
                BusType = x.BusType,
                TotalSeats = x.TotalSeats,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BusDetailResponse> GetWithLayoutAsync(int id, CancellationToken cancellationToken = default)
    {
        var bus = await _context.Buses
            .AsNoTracking()
            .Include(x => x.Operator)
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        return MapDetail(bus, bus.Seats.OrderBy(x => x.Row).ThenBy(x => x.ColumnNumber).Select(MapSeat).ToList());
    }

    public async Task<IReadOnlyList<BusSeatResponse>> GetSeatsAsync(int id, CancellationToken cancellationToken = default)
    {
        var bus = await _context.Buses
            .AsNoTracking()
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        return bus.Seats
            .OrderBy(x => x.Row)
            .ThenBy(x => x.ColumnNumber)
            .Select(MapSeat)
            .ToList();
    }

    public async Task<IReadOnlyList<BusResponse>> GetByOperatorAsync(int userId, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        return await _context.Buses
            .AsNoTracking()
            .Include(x => x.Operator)
            .Where(x => x.OperatorId == operatorId && x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BusResponse
            {
                Id = x.Id,
                OperatorId = x.OperatorId,
                OperatorName = x.Operator.CompanyName,
                BusNumber = x.BusNumber,
                BusType = x.BusType,
                TotalSeats = x.TotalSeats,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BusDetailResponse> CreateAsync(int userId, CreateBusRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        var normalizedNumber = NormalizeBusNumber(request.BusNumber);
        ValidateBusNumber(normalizedNumber);

        var busNumberKey = NormalizeBusNumberKey(normalizedNumber);
        var existingBusNumbers = await _context.Buses
            .AsNoTracking()
            .Select(x => x.BusNumber)
            .ToListAsync(cancellationToken);

        var duplicate = existingBusNumbers.Any(number => NormalizeBusNumberKey(number) == busNumberKey);
        if (duplicate)
        {
            throw new InvalidOperationException("Bus number already exists.");
        }

        var seats = BuildSeatLayout(busType: request.BusType, requestedSeats: request.Seats, busId: 0, fallbackTotalSeats: request.TotalSeats);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var bus = new Bus
        {
            OperatorId = operatorId,
            BusNumber = normalizedNumber,
            BusType = request.BusType,
            TotalSeats = seats.Count,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync(cancellationToken);

        var finalSeats = seats.Select(seat =>
        {
            seat.BusId = bus.Id;
            return seat;
        }).ToList();

        _context.Seats.AddRange(finalSeats);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        var operatorName = await _context.Operators.AsNoTracking().Where(x => x.Id == operatorId).Select(x => x.CompanyName).FirstAsync(cancellationToken);
        return MapDetail(bus, finalSeats.Select(MapSeat).ToList(), operatorName);
    }

    public async Task<BusDetailResponse> UpdateAsync(int userId, int id, UpdateBusRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        var bus = await _context.Buses
            .Include(x => x.Seats)
            .Include(x => x.Operator)
            .FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);

        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        var normalizedNumber = NormalizeBusNumber(request.BusNumber);
        ValidateBusNumber(normalizedNumber);

        var busNumberKey = NormalizeBusNumberKey(normalizedNumber);
        var existingBusNumbers = await _context.Buses
            .AsNoTracking()
            .Where(x => x.Id != id)
            .Select(x => x.BusNumber)
            .ToListAsync(cancellationToken);

        var duplicate = existingBusNumbers.Any(number => NormalizeBusNumberKey(number) == busNumberKey);
        if (duplicate)
        {
            throw new InvalidOperationException("Bus number already exists.");
        }

        var requestedSeats = BuildSeatLayout(request.BusType, request.Seats, bus.Id, request.TotalSeats);
        var layoutChanged = IsLayoutChanged(bus.Seats, requestedSeats);

        if (layoutChanged)
        {
            var hasTrips = await _context.Trips.AnyAsync(x => x.BusId == id, cancellationToken);
            if (hasTrips)
            {
                throw new InvalidOperationException("Cannot change seat layout after the bus has trips assigned.");
            }

            _context.Seats.RemoveRange(bus.Seats);
            await _context.SaveChangesAsync(cancellationToken);

            _context.Seats.AddRange(requestedSeats);
            bus.TotalSeats = requestedSeats.Count;
        }

        bus.BusNumber = normalizedNumber;
        bus.BusType = request.BusType;
        bus.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var seatList = await _context.Seats.AsNoTracking().Where(x => x.BusId == id).OrderBy(x => x.Row).ThenBy(x => x.ColumnNumber).ToListAsync(cancellationToken);
        return MapDetail(bus, seatList.Select(MapSeat).ToList(), bus.Operator.CompanyName);
    }

    public async Task DeactivateAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        var bus = await _context.Buses.FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);
        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        bus.IsActive = false;
        bus.UpdatedAt = DateTime.UtcNow;
        bus.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> GetOperatorIdAsync(int userId, CancellationToken cancellationToken)
    {
        var operatorId = await _context.Operators
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (operatorId == 0)
        {
            throw new KeyNotFoundException("Operator profile not found.");
        }

        return operatorId;
    }

    private static List<Seat> BuildSeatLayout(BusType busType, List<SeatLayoutRequest>? requestedSeats, int busId, int fallbackTotalSeats)
    {
        if (requestedSeats is { Count: > 0 })
        {
            return requestedSeats
                .OrderBy(x => x.Deck)
                .ThenBy(x => x.Row)
                .ThenBy(x => x.ColumnNumber)
                .Select(x => new Seat
                {
                    BusId = busId,
                    SeatNumber = x.SeatNumber.Trim(),
                    Row = x.Row,
                    ColumnNumber = x.ColumnNumber,
                    Deck = x.Deck,
                    SeatType = x.SeatType,
                    IsActive = x.IsActive
                })
                .ToList();
        }

        return GenerateSeats(busId, fallbackTotalSeats, busType);
    }

    private static List<Seat> GenerateSeats(int busId, int totalSeats, BusType busType)
    {
        var seats = new List<Seat>(totalSeats);
        var sleeper = busType is BusType.AC_Sleeper or BusType.Non_AC_Sleeper;
        var seatsPerRow = sleeper ? 2 : 4;

        for (var index = 1; index <= totalSeats; index++)
        {
            var row = (index - 1) / seatsPerRow + 1;
            var column = (index - 1) % seatsPerRow + 1;
            var deck = sleeper ? (row % 2 == 0 ? DeckType.Upper : DeckType.Lower) : DeckType.Single;
            var seatType = seatsPerRow == 2
                ? (column == 1 ? SeatType.Window : SeatType.Aisle)
                : column switch
                {
                    1 => SeatType.Window,
                    2 => SeatType.Aisle,
                    3 => SeatType.Middle,
                    _ => SeatType.Window
                };

            seats.Add(new Seat
            {
                BusId = busId,
                SeatNumber = $"{(sleeper ? deck.ToString()[0] : 'S')}{index}",
                Row = row,
                ColumnNumber = column,
                Deck = deck,
                SeatType = seatType,
                IsActive = true
            });
        }

        return seats;
    }

    private static bool IsLayoutChanged(ICollection<Seat> currentSeats, List<Seat> requestedSeats)
    {
        if (currentSeats.Count != requestedSeats.Count)
        {
            return true;
        }

        var current = currentSeats
            .OrderBy(x => x.Deck)
            .ThenBy(x => x.Row)
            .ThenBy(x => x.ColumnNumber)
            .Select(x => $"{x.SeatNumber}|{x.Row}|{x.ColumnNumber}|{x.Deck}|{x.SeatType}|{x.IsActive}")
            .ToList();

        var requested = requestedSeats
            .OrderBy(x => x.Deck)
            .ThenBy(x => x.Row)
            .ThenBy(x => x.ColumnNumber)
            .Select(x => $"{x.SeatNumber}|{x.Row}|{x.ColumnNumber}|{x.Deck}|{x.SeatType}|{x.IsActive}")
            .ToList();

        for (var index = 0; index < current.Count; index += 1)
        {
            if (!string.Equals(current[index], requested[index], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateBusNumber(string busNumber)
    {
        if (!BusNumberFormat.IsMatch(busNumber))
        {
            throw new ArgumentException("Bus number format must be like TN 37 ET 4632.");
        }
    }

    private static string NormalizeBusNumber(string busNumber)
    {
        var alphanumeric = new string(busNumber.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (alphanumeric.Length != 10)
        {
            return busNumber.Trim().ToUpperInvariant();
        }

        return $"{alphanumeric[..2]} {alphanumeric.Substring(2, 2)} {alphanumeric.Substring(4, 2)} {alphanumeric.Substring(6, 4)}";
    }

    private static string NormalizeBusNumberKey(string busNumber)
    {
        return new string(busNumber.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
    }

    private static BusSeatResponse MapSeat(Seat seat)
    {
        return new BusSeatResponse
        {
            Id = seat.Id,
            BusId = seat.BusId,
            SeatNumber = seat.SeatNumber,
            Row = seat.Row,
            ColumnNumber = seat.ColumnNumber,
            Deck = seat.Deck,
            SeatType = seat.SeatType,
            IsActive = seat.IsActive
        };
    }

    private static BusDetailResponse MapDetail(Bus bus, List<BusSeatResponse> seats, string? operatorName = null)
    {
        return new BusDetailResponse
        {
            Id = bus.Id,
            OperatorId = bus.OperatorId,
            OperatorName = operatorName ?? bus.Operator.CompanyName,
            BusNumber = bus.BusNumber,
            BusType = bus.BusType,
            TotalSeats = bus.TotalSeats,
            IsActive = bus.IsActive,
            CreatedAt = bus.CreatedAt,
            Seats = seats
        };
    }
}
