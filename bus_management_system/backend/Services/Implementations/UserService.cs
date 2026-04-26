using backend.Data;
using backend.Models.DTOs.User;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.Name
        };
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == userId && x.DeletedAt == null, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.Name = request.Name.Trim();
        user.Phone = request.Phone.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.Name
        };
    }

    public async Task<IReadOnlyList<UserBookingResponse>> GetUserBookingsAsync(int userId, string? status, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _context.Bookings
            .AsNoTracking()
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.SourceLocation)
            .Include(x => x.Trip)
                .ThenInclude(x => x.Route)
                    .ThenInclude(x => x.DestinationLocation)
            .Include(x => x.BookingSeats)
            .Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim().ToLowerInvariant();

            switch (normalized)
            {
                case "upcoming":
                    // Confirmed bookings where the trip hasn't departed yet
                    query = query.Where(x =>
                        x.Status == BookingStatus.Confirmed &&
                        x.Trip.DepartureTime >= now);
                    break;

                case "completed":
                    // Confirmed bookings where the trip has already departed
                    query = query.Where(x =>
                        x.Status == BookingStatus.Confirmed &&
                        x.Trip.DepartureTime < now);
                    break;

                case "cancelled":
                    query = query.Where(x => x.Status == BookingStatus.Cancelled);
                    break;

                case "pending":
                    query = query.Where(x => x.Status == BookingStatus.Pending);
                    break;

                case "confirmed":
                    query = query.Where(x => x.Status == BookingStatus.Confirmed);
                    break;

                default:
                    // Ignore unknown filter — return all
                    break;
            }
        }

        return await query
            .OrderByDescending(x => x.BookingDate)
            .Select(x => new UserBookingResponse
            {
                BookingId = x.Id,
                TripId = x.TripId,
                RouteName = x.Trip.Route.SourceLocation.Name + " -> " + x.Trip.Route.DestinationLocation.Name,
                DepartureTime = x.Trip.DepartureTime,
                ArrivalTime = x.Trip.ArrivalTime,
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                BookingDate = x.BookingDate,
                // Only count active (non-cancelled) seats
                SeatCount = x.BookingSeats.Count(s => s.Status != BookingSeatStatus.Cancelled)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UserBookingDetailResponse> GetBookingDetailAsync(int userId, int bookingId, CancellationToken cancellationToken = default)
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

        var paymentStatus = booking.Payments
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.PaymentStatus)
            .FirstOrDefault();

        return new UserBookingDetailResponse
        {
            BookingId = booking.Id,
            TripId = booking.TripId,
            RouteName = booking.Trip.Route.SourceLocation.Name + " -> " + booking.Trip.Route.DestinationLocation.Name,
            DepartureTime = booking.Trip.DepartureTime,
            ArrivalTime = booking.Trip.ArrivalTime,
            TotalAmount = booking.TotalAmount,
            PlatformFee = booking.PlatformFee,
            Status = booking.Status,
            BookingDate = booking.BookingDate,
            CancelledAt = booking.CancelledAt,
            CancelReason = booking.CancelReason,
            PaymentStatus = paymentStatus,
            Seats = booking.BookingSeats
                .OrderBy(x => x.Id)
                .Select(x => new UserBookingSeatResponse
                {
                    BookingSeatId = x.Id,
                    SeatId = x.SeatId,
                    SeatNumber = x.Seat.SeatNumber,
                    AmountPaid = x.AmountPaid,
                    Status = x.Status,
                    PassengerName = x.Passenger != null ? x.Passenger.Name : null,
                    PassengerAge = x.Passenger != null ? x.Passenger.Age : null,
                    PassengerGender = x.Passenger != null ? x.Passenger.Gender : null
                })
                .ToList()
        };
    }

    public async Task<IReadOnlyList<UserNotificationResponse>> GetUserNotificationsAsync(int userId, bool unread, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (unread)
        {
            query = query.Where(x => !x.IsRead);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserNotificationResponse
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                Type = x.Type,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Channel = x.Channel,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkReadAsync(int userId, int id, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found.");
        }

        if (notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadNotifications.Count == 0)
        {
            return;
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
