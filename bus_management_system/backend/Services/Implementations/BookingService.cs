using backend.Data;
using backend.Models.DTOs.Booking;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReserveSeatResponse> ReserveSeatAsync(
        int userId,
        ReserveSeatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SeatIds == null || request.SeatIds.Count == 0)
        {
            throw new ArgumentException("At least one seat must be selected.");
        }

        var seatIds = request.SeatIds.Distinct().ToList();
        var trip = await _context.Trips
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.TripId && x.DeletedAt == null, cancellationToken);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found.");
        }

        var tripSeats = await _context.TripSeats
            .Where(x => x.TripId == request.TripId && seatIds.Contains(x.SeatId))
            .ToListAsync(cancellationToken);

        if (tripSeats.Count != seatIds.Count)
        {
            throw new ArgumentException("One or more requested seats do not exist for this trip.");
        }

        var unavailable = tripSeats.Where(x => x.Status != SeatStatus.Available).ToList();
        if (unavailable.Count > 0)
        {
            throw new InvalidOperationException("One or more selected seats are not available.");
        }

        var lockedUntil = DateTime.UtcNow.AddMinutes(5);
        foreach (var tripSeat in tripSeats)
        {
            tripSeat.Status = SeatStatus.Reserved;
            tripSeat.LockedBy = userId;
            tripSeat.LockedUntil = lockedUntil;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ReserveSeatResponse
        {
            TripId = request.TripId,
            LockedUntil = lockedUntil,
            ReservedTripSeatIds = tripSeats.Select(x => x.Id).ToList()
        };
    }

public async Task<BookingResponse> CreateBookingAsync(
    int userId,
    CreateBookingRequest request,
    CancellationToken cancellationToken = default)
{
    if (request.Passengers == null || request.Passengers.Count == 0)
        throw new ArgumentException("At least one passenger must be specified.");

    if (request.Passengers.GroupBy(x => x.TripSeatId).Any(x => x.Count() > 1))
        throw new ArgumentException("Duplicate trip seat ids are not allowed.");

    var trip = await _context.Trips
        .Include(x => x.Route).ThenInclude(x => x.SourceLocation)
        .Include(x => x.Route).ThenInclude(x => x.DestinationLocation)
        .Include(x => x.Operator)
        .FirstOrDefaultAsync(x => x.Id == request.TripId && x.DeletedAt == null, cancellationToken);

    if (trip == null)
        throw new KeyNotFoundException("Trip not found.");

    var boardingPointExists = await _context.BoardingPoints
        .AnyAsync(x => x.Id == request.BoardingPointId && x.RouteId == trip.RouteId, cancellationToken);

    if (!boardingPointExists)
        throw new ArgumentException("Invalid boarding point for this trip.");

    var droppingPointExists = await _context.DroppingPoints
        .AnyAsync(x => x.Id == request.DroppingPointId && x.RouteId == trip.RouteId, cancellationToken);

    if (!droppingPointExists)
        throw new ArgumentException("Invalid dropping point for this trip.");

    var tripSeatIds = request.Passengers.Select(x => x.TripSeatId).ToList();

    var tripSeats = await _context.TripSeats
        .Include(x => x.Seat)
        .Where(x => x.TripId == request.TripId && tripSeatIds.Contains(x.Id))
        .ToListAsync(cancellationToken);

    if (tripSeats.Count != tripSeatIds.Count)
        throw new ArgumentException("One or more reserved seats were not found for this trip.");

    var now = DateTime.UtcNow;
    var invalidSeats = tripSeats.Where(x =>
        x.Status != SeatStatus.Reserved ||
        x.LockedBy != userId ||
        x.LockedUntil == null ||
        x.LockedUntil <= now).ToList();

    if (invalidSeats.Count > 0)
        throw new InvalidOperationException("One or more seats are not reserved by the current user or reservation has expired.");

    var seatPriceMap = await _context.SeatPricing
        .AsNoTracking()
        .Where(x => x.TripId == request.TripId && tripSeats.Select(ts => ts.SeatId).Contains(x.SeatId))
        .ToDictionaryAsync(x => x.SeatId, x => x.Price, cancellationToken);

    // ✅ Fetch active platform fee
    var feeConfig = await _context.PlatformFeeConfigs
        .AsNoTracking()
        .Where(x => x.IsActive)
        .OrderByDescending(x => x.CreatedAt)
        .FirstOrDefaultAsync(cancellationToken);

    var feePercent = feeConfig?.FeeValue ?? 0m;

    await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

    var booking = new Booking
    {
        UserId = userId,
        TripId = request.TripId,
        BoardingPointId = request.BoardingPointId,
        DroppingPointId = request.DroppingPointId,
        Status = BookingStatus.Pending,
        BookingDate = now,
        CreatedAt = now,
        UpdatedAt = now
    };

    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync(cancellationToken);

    var bookingSeats = new List<BookingSeat>();
    foreach (var passenger in request.Passengers)
    {
        var tripSeat = tripSeats.First(x => x.Id == passenger.TripSeatId);
        var amount = seatPriceMap.TryGetValue(tripSeat.SeatId, out var seatPrice)
            ? seatPrice
            : trip.BaseFare;

        // ✅ Extend lock until payment completes (15 min)
        tripSeat.LockedUntil = now.AddMinutes(15);

        bookingSeats.Add(new BookingSeat
        {
            BookingId = booking.Id,
            TripSeatId = tripSeat.Id,
            SeatId = tripSeat.SeatId,
            AmountPaid = amount,
            Status = BookingSeatStatus.Confirmed,
            Passenger = new Passenger
            {
                Name = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = Enum.Parse<Gender>(passenger.Gender, true)
            }
        });
    }

    _context.BookingSeats.AddRange(bookingSeats);

    // ✅ Calculate platform fee correctly
var totalAmount = bookingSeats.Sum(x => x.AmountPaid);
var platformFee = Math.Round(totalAmount * feePercent / 100, 2);

booking.TotalAmount = totalAmount + platformFee;  // ✅ include fee in total
booking.PlatformFee = platformFee;

    await _context.SaveChangesAsync(cancellationToken);
    await tx.CommitAsync(cancellationToken);

    return new BookingResponse
    {
        BookingId = booking.Id,
        TripDetails = $"{trip.Route.SourceLocation.Name} -> {trip.Route.DestinationLocation.Name}",
        TotalAmount = booking.TotalAmount,
        PlatformFee = booking.PlatformFee,   // ✅ include in response
        Status = booking.Status,
        BookingDate = booking.BookingDate
    };
}

    public async Task<BookingDetailResponse> GetBookingDetailAsync(
        int userId,
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Seat)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.Passenger)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId, cancellationToken);

        if (booking == null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        var latestPaymentStatus = booking.Payments
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.PaymentStatus)
            .FirstOrDefault();

        return new BookingDetailResponse
        {
            BookingId = booking.Id,
            TripId = booking.TripId,
            RouteName = $"{booking.Trip.Route.SourceLocation.Name} -> {booking.Trip.Route.DestinationLocation.Name}",
            DepartureTime = booking.Trip.DepartureTime,
            ArrivalTime = booking.Trip.ArrivalTime,
            TotalAmount = booking.TotalAmount,
            PlatformFee = booking.PlatformFee,
            Status = booking.Status,
            PaymentStatus = latestPaymentStatus,
            BookingDate = booking.BookingDate,
            Seats = booking.BookingSeats
                .OrderBy(x => x.Id)
                .Select(x => new BookingSeatDetail
                {
                    BookingSeatId = x.Id,
                    TripSeatId = x.TripSeatId,
                    SeatId = x.SeatId,
                    SeatNumber = x.Seat.SeatNumber,
                    AmountPaid = x.AmountPaid,
                    Status = x.Status,
                    PassengerName = x.Passenger != null ? x.Passenger.Name : "Unknown",
                    PassengerAge = x.Passenger != null ? x.Passenger.Age : 0,
                    PassengerGender = x.Passenger != null ? x.Passenger.Gender.ToString() : "Unknown"
                })
                .ToList()
        };
    }

    public async Task<BookingResponse> CancelBookingAsync(
        int userId,
        int bookingId,
        string cancelReason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cancelReason))
        {
            throw new ArgumentException("Cancel reason is required.");
        }

        var booking = await _context.Bookings
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.TripSeat)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId, cancellationToken);

        if (booking == null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancelReason = cancelReason.Trim();
        // Platform fee is non-refundable. The total amount now only consists of the retained platform fee.
        booking.TotalAmount = booking.PlatformFee;
        booking.UpdatedAt = DateTime.UtcNow;

        foreach (var bookingSeat in booking.BookingSeats)
        {
            if (bookingSeat.Status != BookingSeatStatus.Cancelled)
            {
                bookingSeat.Status = BookingSeatStatus.Cancelled;
                bookingSeat.CancelledAt = DateTime.UtcNow;
            }

            bookingSeat.TripSeat.Status = SeatStatus.Available;
            bookingSeat.TripSeat.LockedUntil = null;
            bookingSeat.TripSeat.LockedBy = null;
        }

        await SyncTransactionAsync(booking.Id, booking.TotalAmount, booking.PlatformFee, cancellationToken);

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Booking Cancelled",
            Message = $"Booking #{bookingId} cancelled. Refund will be processed within 5-7 business days.",
            Type = "booking_cancelled",
            ReferenceType = ReferenceType.Booking,
            ReferenceId = bookingId,
            Channel = NotificationChannel.InApp,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new BookingResponse
        {
            BookingId = booking.Id,
            TripDetails = $"{booking.Trip.Route.SourceLocation.Name} -> {booking.Trip.Route.DestinationLocation.Name}",
            TotalAmount = booking.TotalAmount,
            PlatformFee = booking.PlatformFee,
            Status = booking.Status,
            BookingDate = booking.BookingDate
        };
    }

    public async Task<CancelSeatResponse> CancelSeatAsync(
        int userId,
        int bookingId,
        int bookingSeatId,
        string cancelReason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cancelReason))
        {
            throw new ArgumentException("Cancel reason is required.");
        }

        var booking = await _context.Bookings
            .Include(x => x.BookingSeats)
                .ThenInclude(x => x.TripSeat)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.UserId == userId, cancellationToken);

        if (booking == null)
        {
            throw new KeyNotFoundException("Booking not found.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot cancel seat for a cancelled booking.");
        }

        var bookingSeat = booking.BookingSeats.FirstOrDefault(x => x.Id == bookingSeatId);
        if (bookingSeat == null)
        {
            throw new KeyNotFoundException("Booking seat not found.");
        }

        if (bookingSeat.Status == BookingSeatStatus.Cancelled)
        {
            throw new InvalidOperationException("Booking seat is already cancelled.");
        }

        bookingSeat.Status = BookingSeatStatus.Cancelled;
        bookingSeat.CancelledAt = DateTime.UtcNow;

        bookingSeat.TripSeat.Status = SeatStatus.Available;
        bookingSeat.TripSeat.LockedUntil = null;
        bookingSeat.TripSeat.LockedBy = null;

        var activeSeats = booking.BookingSeats.Where(x => x.Status != BookingSeatStatus.Cancelled).ToList();
        var seatSubtotal = activeSeats.Sum(x => x.AmountPaid);

        // Platform fee is non-refundable, keep original PlatformFee
        booking.TotalAmount = seatSubtotal + booking.PlatformFee;
        booking.UpdatedAt = DateTime.UtcNow;

        if (activeSeats.Count == 0)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancelReason = cancelReason.Trim();
        }

        await SyncTransactionAsync(booking.Id, booking.TotalAmount, booking.PlatformFee, cancellationToken);

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = "Seat Cancelled",
            Message = $"Seat from booking #{bookingId} cancelled. Refund of \u20b9{bookingSeat.AmountPaid:N0} will be processed within 5-7 business days.",
            Type = "seat_cancelled",
            ReferenceType = ReferenceType.Booking,
            ReferenceId = bookingId,
            Channel = NotificationChannel.InApp,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new CancelSeatResponse
        {
            BookingId = booking.Id,
            BookingSeatId = bookingSeat.Id,
            BookingStatus = booking.Status,
            UpdatedTotalAmount = booking.TotalAmount
        };
    }

    private async Task SyncTransactionAsync(int bookingId, decimal totalAmount, decimal platformFee, CancellationToken cancellationToken)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(x => x.BookingId == bookingId, cancellationToken);

        if (transaction == null)
        {
            return;
        }

        transaction.TotalAmount = totalAmount;
        transaction.PlatformFee = platformFee;
        transaction.OperatorEarning = Math.Max(0, totalAmount - platformFee);
        transaction.UpdatedAt = DateTime.UtcNow;
    }
}
