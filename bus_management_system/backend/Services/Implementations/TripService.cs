using backend.Data;
using backend.Jobs;
using backend.Models.DTOs.Trip;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly TripGeneratorJob _tripGeneratorJob;

    public TripService(AppDbContext context, TripGeneratorJob tripGeneratorJob)
    {
        _context = context;
        _tripGeneratorJob = tripGeneratorJob;
    }

    public async Task<IReadOnlyList<TripResponse>> SearchAsync(TripSearchRequest request, CancellationToken cancellationToken = default)
    {
        // Legacy mode active: Unspecified Kind maps to 'timestamp without time zone'
        // so the comparison is a plain date comparison with no timezone conversion.
        var d = request.TravelDate;
        var start = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
        var end = start.AddDays(1);



        var query = _context.Trips
            .AsNoTracking()
            .Include(x => x.Bus)
            .Include(x => x.Operator)
            .Include(x => x.Route)
                .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Route)
                .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.TripSeats)
            .Where(x => x.DepartureTime >= start && x.DepartureTime < end && x.Status != TripStatus.Cancelled && x.DeletedAt == null && x.Route.SourceId == request.SourceId && x.Route.DestinationId == request.DestinationId);

        if (request.BusType.HasValue)
        {
            query = query.Where(x => x.Bus.BusType == request.BusType.Value);
        }

        return await query
            .OrderBy(x => x.DepartureTime)
            .Select(x => new TripResponse
            {
                TripId = x.Id,
                OperatorName = x.Operator.CompanyName,
                BusNumber = x.Bus.BusNumber,
                BusType = x.Bus.BusType,
                RouteName = x.Route.SourceLocation.Name + " -> " + x.Route.DestinationLocation.Name,
                DepartureTime = x.DepartureTime,
                ArrivalTime = x.ArrivalTime,
                BaseFare = x.BaseFare,
                AvailableSeats = x.TripSeats.Count(s => s.Status == SeatStatus.Available),
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TripDetailResponse> GetDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .AsNoTracking()
            .Include(x => x.Bus)
            .Include(x => x.Operator)
            .Include(x => x.Route)
                .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Route)
                .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.TripSeats)
                .ThenInclude(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var priceMap = await GetSeatPriceMapAsync(id, cancellationToken);
        return MapTripDetail(trip, priceMap);
    }

    public async Task<IReadOnlyList<TripSeatResponse>> GetAvailableSeatsAsync(int id, CancellationToken cancellationToken = default)
    {
        var trip = await _context.Trips
            .AsNoTracking()
            .Include(x => x.TripSeats)
                .ThenInclude(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var priceMap = await GetSeatPriceMapAsync(id, cancellationToken);
        return BuildSeatResponses(trip, priceMap).Where(x => x.Status == SeatStatus.Available).ToList();
    }

public async Task<TripPointsResponse> GetPointsAsync(int id, CancellationToken cancellationToken = default)
{
    var trip = await _context.Trips
        .AsNoTracking()
        .Include(x => x.Route)
        .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null, cancellationToken);

    if (trip == null) throw new KeyNotFoundException("Trip not found.");

    var boardingPoints = await _context.BoardingPoints
        .AsNoTracking()
        .Include(x => x.Location)
        .Where(x => x.RouteId == trip.RouteId)
        .OrderByDescending(x => x.IsDefault)
        .ThenBy(x => x.TimeOffset)
        .ThenBy(x => x.Name)
        .Select(x => new TripPointResponse
        {
            Id = x.Id,
            LocationId = x.LocationId,
            LocationName = x.Location.Name + ", " + x.Location.City + ", " + x.Location.State,
            Name = x.Name,
            TimeOffset = x.TimeOffset,
            IsDefault = x.IsDefault
        })
        .ToListAsync(cancellationToken);

    var droppingPoints = await _context.DroppingPoints
        .AsNoTracking()
        .Include(x => x.Location)
        .Where(x => x.RouteId == trip.RouteId)
        .OrderByDescending(x => x.IsDefault)
        .ThenBy(x => x.TimeOffset)
        .ThenBy(x => x.Name)
        .Select(x => new TripPointResponse
        {
            Id = x.Id,
            LocationId = x.LocationId,
            LocationName = x.Location.Name + ", " + x.Location.City + ", " + x.Location.State,
            Name = x.Name,
            TimeOffset = x.TimeOffset,
            IsDefault = x.IsDefault
        })
        .ToListAsync(cancellationToken);

    return new TripPointsResponse
    {
        BoardingPoints = boardingPoints,
        DroppingPoints = droppingPoints
    };
}


    public async Task<IReadOnlyList<OperatorTripResponse>> GetByOperatorAsync(int userId, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        return await _context.Trips
            .AsNoTracking()
            .Include(x => x.Bus)
            .Include(x => x.Route)
                .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Route)
                .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.TripSeats)
            .Where(x => x.OperatorId == operatorId && x.DeletedAt == null)
            .OrderByDescending(x => x.DepartureTime)
            .Select(x => new OperatorTripResponse
            {
                TripId = x.Id,
                ScheduleId = x.ScheduleId ?? 0,
                BusNumber = x.Bus.BusNumber,
                RouteName = x.Route.SourceLocation.Name + " -> " + x.Route.DestinationLocation.Name,
                DepartureTime = x.DepartureTime,
                ArrivalTime = x.ArrivalTime,
                BaseFare = x.BaseFare,
                Status = x.Status,
                AvailableSeats = x.TripSeats.Count(s => s.Status == SeatStatus.Available),
                BookedSeats = x.TripSeats.Count(s => s.Status == SeatStatus.Booked)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TripScheduleResponse>> GetSchedulesByOperatorAsync(int userId, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        return await _context.TripSchedules
            .AsNoTracking()
            .Where(x => x.OperatorId == operatorId)
            .OrderByDescending(x => x.ValidFrom)
            .ThenBy(x => x.DepartureTime)
            .Select(x => new TripScheduleResponse
            {
                Id = x.Id,
                BusId = x.BusId,
                RouteId = x.RouteId,
                OperatorId = x.OperatorId,
                DepartureTime = x.DepartureTime,
                ArrivalTime = x.ArrivalTime,
                BaseFare = x.BaseFare,
                DaysOfWeek = x.DaysOfWeek,
                ValidFrom = x.ValidFrom,
                ValidUntil = x.ValidUntil,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<TripScheduleResponse> CreateScheduleAsync(int userId, CreateScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var bus = await _context.Buses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BusId && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);
        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        // Check #1: a bus can only be on ONE active schedule at a time
        var busAlreadyScheduled = await _context.TripSchedules
            .AsNoTracking()
            .AnyAsync(x => x.BusId == request.BusId && x.IsActive, cancellationToken);
        if (busAlreadyScheduled)
        {
            throw new InvalidOperationException(
                $"Bus '{bus.BusNumber}' already has an active schedule. Pause or delete the existing schedule before creating a new one.");
        }

        var route = await _context.Routes
            .AsNoTracking()
            .Include(x => x.SourceLocation)
            .Include(x => x.DestinationLocation)
            .FirstOrDefaultAsync(x => x.Id == request.RouteId && x.IsActive, cancellationToken);
        if (route == null)
        {
            throw new KeyNotFoundException("Route not found.");
        }

        // Check #2: operator must have offices at source (boarding) and destination (dropping) locations
        await ValidateOperatorOfficesForRouteAsync(operatorId, route, cancellationToken);

        var schedule = new TripSchedule
        {
            BusId = request.BusId,
            RouteId = request.RouteId,
            OperatorId = operatorId,
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            BaseFare = request.BaseFare,
            DaysOfWeek = request.DaysOfWeek.Trim(),
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TripSchedules.Add(schedule);
        await _context.SaveChangesAsync(cancellationToken);

        await EnsureBoardingDroppingPointsAsync(operatorId, route, cancellationToken);

        // Generate today's trip synchronously so it's ready when the frontend reloads
        await _tripGeneratorJob.ProcessSchedulesAsync(cancellationToken);

        return MapSchedule(schedule);
    }

    public async Task<TripScheduleResponse> UpdateScheduleAsync(int userId, int id, UpdateScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var schedule = await _context.TripSchedules.FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId, cancellationToken);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Schedule not found.");
        }

        var bus = await _context.Buses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.BusId && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);
        if (bus == null)
        {
            throw new KeyNotFoundException("Bus not found.");
        }

        // Check #1: bus can only have one active schedule (exclude the current schedule being edited)
        var busAlreadyScheduled = await _context.TripSchedules
            .AsNoTracking()
            .AnyAsync(x => x.BusId == request.BusId && x.IsActive && x.Id != id, cancellationToken);
        if (busAlreadyScheduled)
        {
            throw new InvalidOperationException(
                $"Bus '{bus.BusNumber}' already has an active schedule. Pause or delete the existing schedule before assigning it here.");
        }

        var route = await _context.Routes
            .AsNoTracking()
            .Include(x => x.SourceLocation)
            .Include(x => x.DestinationLocation)
            .FirstOrDefaultAsync(x => x.Id == request.RouteId && x.IsActive, cancellationToken);
        if (route == null)
        {
            throw new KeyNotFoundException("Route not found.");
        }

        // Check #2: operator must have offices at source and destination of the route
        await ValidateOperatorOfficesForRouteAsync(operatorId, route, cancellationToken);

        schedule.BusId = request.BusId;
        schedule.RouteId = request.RouteId;
        schedule.DepartureTime = request.DepartureTime;
        schedule.ArrivalTime = request.ArrivalTime;
        schedule.BaseFare = request.BaseFare;
        schedule.DaysOfWeek = request.DaysOfWeek.Trim();
        schedule.ValidFrom = request.ValidFrom;
        schedule.ValidUntil = request.ValidUntil;
        schedule.IsActive = request.IsActive;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Re-run generator synchronously so trips reflect the updated schedule immediately
        await _tripGeneratorJob.ProcessSchedulesAsync(cancellationToken);

        return MapSchedule(schedule);
    }

    public async Task<TripScheduleResponse> ToggleScheduleAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var schedule = await _context.TripSchedules.FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId, cancellationToken);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Schedule not found.");
        }

        schedule.IsActive = !schedule.IsActive;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return MapSchedule(schedule);
    }

    public async Task<TripDetailResponse> ChangeBusAsync(int userId, int id, ChangeBusRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var trip = await _context.Trips
            .Include(x => x.Route)
                .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Route)
                .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.Bus)
            .Include(x => x.Operator)
            .Include(x => x.TripSeats)
                .ThenInclude(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var hasBookings = await _context.Bookings.AnyAsync(x => x.TripId == id && x.Status == BookingStatus.Confirmed, cancellationToken);
        if (hasBookings)
        {
            throw new InvalidOperationException("Cannot change bus for a trip that already has confirmed bookings.");
        }

        var newBus = await _context.Buses
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == request.NewBusId && x.OperatorId == operatorId && x.DeletedAt == null && x.IsActive, cancellationToken);

        if (newBus == null)
        {
            throw new KeyNotFoundException("New bus not found.");
        }

        var existingTripSeats = await _context.TripSeats.Where(x => x.TripId == id).ToListAsync(cancellationToken);
        var oldBusId = trip.BusId;
        _context.TripSeats.RemoveRange(existingTripSeats);
        _context.SeatPricing.RemoveRange(_context.SeatPricing.Where(x => x.TripId == id));

        var busChange = new BusChange
        {
            TripId = id,
            OldBusId = oldBusId,
            NewBusId = newBus.Id,
            ChangeType = ChangeType.Permanent,
            Reason = request.Reason.Trim(),
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow
        };

        _context.BusChanges.Add(busChange);

        trip.BusId = newBus.Id;
        trip.UpdatedAt = DateTime.UtcNow;
        trip.Status = TripStatus.Scheduled;

        var newTripSeats = newBus.Seats.Select(seat => new TripSeat
        {
            TripId = trip.Id,
            SeatId = seat.Id,
            Status = SeatStatus.Available,
            LockedUntil = null,
            LockedBy = null
        }).ToList();

        _context.TripSeats.AddRange(newTripSeats);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetDetailAsync(id, cancellationToken);
    }

    public async Task<TripDetailResponse> CancelAsync(int userId, int id, CancelTripRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var trip = await _context.Trips
            .Include(x => x.Bus)
            .Include(x => x.Operator)
            .Include(x => x.Route)
                .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Route)
                .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.TripSeats)
                .ThenInclude(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId && x.DeletedAt == null, cancellationToken);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var hasBookings = await _context.Bookings.AnyAsync(x => x.TripId == id && x.Status == BookingStatus.Confirmed, cancellationToken);
        if (hasBookings)
        {
            throw new InvalidOperationException("Cannot cancel a trip with confirmed bookings.");
        }

        trip.Status = TripStatus.Cancelled;
        trip.CancellationReason = request.Reason.Trim();
        trip.UpdatedAt = DateTime.UtcNow;

        foreach (var seat in trip.TripSeats)
        {
            seat.Status = SeatStatus.Available;
            seat.LockedBy = null;
            seat.LockedUntil = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        var priceMap = await GetSeatPriceMapAsync(id, cancellationToken);
        return MapTripDetail(trip, priceMap);
    }

    public async Task<IReadOnlyList<SeatPricingResponse>> GetSeatPricingAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var trip = await _context.Trips.AsNoTracking().Include(x => x.Bus).FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId, cancellationToken);
        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        return await _context.SeatPricing
            .AsNoTracking()
            .Include(x => x.Seat)
            .Where(x => x.TripId == id)
            .OrderBy(x => x.Seat.SeatNumber)
            .Select(x => new SeatPricingResponse
            {
                SeatId = x.SeatId,
                SeatNumber = x.Seat.SeatNumber,
                Price = x.Price
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SeatPricingResponse>> SetSeatPricingAsync(int userId, int id, SetSeatPricingRequest request, CancellationToken cancellationToken = default)
    {
        var operatorId = await GetOperatorIdAsync(userId, cancellationToken);
        var trip = await _context.Trips.Include(x => x.Bus).FirstOrDefaultAsync(x => x.Id == id && x.OperatorId == operatorId, cancellationToken);
        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var seatIds = request.Prices.Select(x => x.SeatId).Distinct().ToList();
        var busSeatIds = await _context.Seats.Where(x => x.BusId == trip.BusId && seatIds.Contains(x.Id)).Select(x => x.Id).ToListAsync(cancellationToken);
        if (busSeatIds.Count != seatIds.Count)
        {
            throw new ArgumentException("One or more seat ids are invalid for this trip bus.");
        }

        var existing = await _context.SeatPricing.Where(x => x.TripId == id && seatIds.Contains(x.SeatId)).ToListAsync(cancellationToken);
        _context.SeatPricing.RemoveRange(existing);

        var toAdd = request.Prices.Select(x => new SeatPricing
        {
            TripId = id,
            SeatId = x.SeatId,
            Price = x.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _context.SeatPricing.AddRange(toAdd);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetSeatPricingAsync(userId, id, cancellationToken);
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

    /// <summary>
    /// Verifies the operator has at least one office in the route's source location (boarding)
    /// and at least one office in the destination location (dropping).
    /// </summary>
    private async Task ValidateOperatorOfficesForRouteAsync(int operatorId, backend.Models.Entities.Route route, CancellationToken cancellationToken)
    {
        var officeLocationIds = await _context.OperatorOffices
            .AsNoTracking()
            .Where(x => x.OperatorId == operatorId)
            .Select(x => x.LocationId)
            .ToListAsync(cancellationToken);

        if (!officeLocationIds.Contains(route.SourceId))
        {
            throw new InvalidOperationException(
                $"You don't have an office at the boarding location '{route.SourceLocation.Name}'. " +
                "Please add an office at that location before creating a schedule for this route.");
        }

        if (!officeLocationIds.Contains(route.DestinationId))
        {
            throw new InvalidOperationException(
                $"You don't have an office at the dropping location '{route.DestinationLocation.Name}'. " +
                "Please add an office at that location before creating a schedule for this route.");
        }
    }

    private async Task<Dictionary<int, decimal>> GetSeatPriceMapAsync(int tripId, CancellationToken cancellationToken)
    {
        return await _context.SeatPricing
            .AsNoTracking()
            .Where(x => x.TripId == tripId)
            .ToDictionaryAsync(x => x.SeatId, x => x.Price, cancellationToken);
    }

    private TripDetailResponse MapTripDetail(Trip trip, IReadOnlyDictionary<int, decimal> priceMap)
    {
        return new TripDetailResponse
        {
            TripId = trip.Id,
            OperatorName = trip.Operator.CompanyName,
            BusNumber = trip.Bus.BusNumber,
            BusType = trip.Bus.BusType,
            RouteName = trip.Route.SourceLocation.Name + " -> " + trip.Route.DestinationLocation.Name,
            DepartureTime = trip.DepartureTime,
            ArrivalTime = trip.ArrivalTime,
            BaseFare = trip.BaseFare,
            AvailableSeats = trip.TripSeats.Count(s => s.Status == SeatStatus.Available),
            Status = trip.Status,
            RouteId = trip.RouteId,
            SourceName = trip.Route.SourceLocation.Name + ", " + trip.Route.SourceLocation.City + ", " + trip.Route.SourceLocation.State,
            DestinationName = trip.Route.DestinationLocation.Name + ", " + trip.Route.DestinationLocation.City + ", " + trip.Route.DestinationLocation.State,
            Seats = BuildSeatResponses(trip, priceMap)
        };
    }

    private static List<TripSeatResponse> BuildSeatResponses(Trip trip, IReadOnlyDictionary<int, decimal> priceMap)
    {
        return trip.TripSeats
            .OrderBy(x => x.Seat.Row)
            .ThenBy(x => x.Seat.ColumnNumber)
            .Select(x => new TripSeatResponse
            {
                TripSeatId = x.Id,
                SeatId = x.SeatId,
                SeatNumber = x.Seat.SeatNumber,
                Row = x.Seat.Row,
                ColumnNumber = x.Seat.ColumnNumber,
                Deck = x.Seat.Deck,
                SeatType = x.Seat.SeatType,
                Status = x.Status,
                LockedUntil = x.LockedUntil,
                Price = priceMap.TryGetValue(x.SeatId, out var price) ? price : trip.BaseFare
            })
            .ToList();
    }

    private async Task EnsureBoardingDroppingPointsAsync(
    int operatorId,
    backend.Models.Entities.Route route,
    CancellationToken cancellationToken)
{
    // Check if boarding points already exist for this route
    var hasBoardingPoints = await _context.BoardingPoints
        .AnyAsync(x => x.RouteId == route.Id, cancellationToken);

    if (!hasBoardingPoints)
    {
        // Get operator's office at source location
        var sourceOffice = await _context.OperatorOffices
            .AsNoTracking()
            .Where(x => x.OperatorId == operatorId && x.LocationId == route.SourceId)
            .OrderByDescending(x => x.IsHeadOffice)
            .FirstOrDefaultAsync(cancellationToken);

        if (sourceOffice != null)
        {
            _context.BoardingPoints.Add(new BoardingPoint
            {
                RouteId    = route.Id,
                LocationId = route.SourceId,
                OfficeId   = sourceOffice.Id,
                Name       = sourceOffice.Address,
                TimeOffset = 0,
                IsDefault  = true
            });
        }
    }

    // Check if dropping points already exist for this route
    var hasDroppingPoints = await _context.DroppingPoints
        .AnyAsync(x => x.RouteId == route.Id, cancellationToken);

    if (!hasDroppingPoints)
    {
        // Get operator's office at destination location
        var destOffice = await _context.OperatorOffices
            .AsNoTracking()
            .Where(x => x.OperatorId == operatorId && x.LocationId == route.DestinationId)
            .OrderByDescending(x => x.IsHeadOffice)
            .FirstOrDefaultAsync(cancellationToken);

        if (destOffice != null)
        {
            _context.DroppingPoints.Add(new DroppingPoint
            {
                RouteId    = route.Id,
                LocationId = route.DestinationId,
                OfficeId   = destOffice.Id,
                Name       = destOffice.Address,
                TimeOffset = 0,
                IsDefault  = true
            });
        }
    }

    await _context.SaveChangesAsync(cancellationToken);
}

    private static TripScheduleResponse MapSchedule(TripSchedule schedule)
    {
        return new TripScheduleResponse
        {
            Id = schedule.Id,
            BusId = schedule.BusId,
            RouteId = schedule.RouteId,
            OperatorId = schedule.OperatorId,
            DepartureTime = schedule.DepartureTime,
            ArrivalTime = schedule.ArrivalTime,
            BaseFare = schedule.BaseFare,
            DaysOfWeek = schedule.DaysOfWeek,
            ValidFrom = schedule.ValidFrom,
            ValidUntil = schedule.ValidUntil,
            IsActive = schedule.IsActive
        };
    }
}
