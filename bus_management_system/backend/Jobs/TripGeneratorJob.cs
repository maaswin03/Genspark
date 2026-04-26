using backend.Data;
using backend.Models.Entities;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Jobs;

/// <summary>
/// Background job that runs daily to generate trips from active schedules.
/// Creates a Trip for each day matching the schedule's DaysOfWeek.
/// Also creates TripSeats for all active seats in the bus.
/// </summary>
public class TripGeneratorJob
{
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _jobTask;

    public TripGeneratorJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _jobTask = RunAsync(_cancellationTokenSource.Token);
    }

    public async Task StopAsync()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            if (_jobTask != null)
            {
                try
                {
                    await _jobTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping the job
                }
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[TripGeneratorJob] Started. Runs every hour and immediately on new schedules.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Console.WriteLine($"[TripGeneratorJob] Running at {DateTime.Now:u}");
                await ProcessSchedulesAsync(cancellationToken);
                Console.WriteLine($"[TripGeneratorJob] Finished at {DateTime.Now:u}");

                // Re-run every hour (not just at midnight) — same approach as SeatLockExpiryJob
                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TripGeneratorJob] Error: {ex.Message}");
                // Retry after 5 minutes on error
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
            }
        }
    }

    public async Task ProcessSchedulesAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateOnly.FromDateTime(DateTime.Now);

            // Find all active schedules
            var activeSchedules = await context.TripSchedules
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.Bus)
                    .ThenInclude(x => x.Seats)
                .ToListAsync(cancellationToken);

            foreach (var schedule in activeSchedules)
            {
                // Check if schedule is valid for today
                if (today < schedule.ValidFrom || (schedule.ValidUntil.HasValue && today > schedule.ValidUntil))
                {
                    continue; // Schedule not active on this date
                }

                // Check if today's day of week is in the schedule's DaysOfWeek list
                if (!IsScheduleActiveOnDayOfWeek(today, schedule.DaysOfWeek))
                {
                    continue;
                }

                // Check if a trip already exists for this schedule today
                var dayStart = DateTime.SpecifyKind(today.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
                var nextDayStart = DateTime.SpecifyKind(today.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);

                var existingTrip = await context.Trips
                    .AnyAsync(
                        x => x.ScheduleId == schedule.Id &&
                             x.DepartureTime >= dayStart &&
                             x.DepartureTime < nextDayStart,
                        cancellationToken);

                if (existingTrip)
                {
                    continue; // Trip already exists for today
                }

                // Create the trip for today
                var tripDateTime = DateTime.SpecifyKind(today.ToDateTime(TimeOnly.FromTimeSpan(schedule.DepartureTime)), DateTimeKind.Unspecified);
                var arrivalDateTime = DateTime.SpecifyKind(today.ToDateTime(TimeOnly.FromTimeSpan(schedule.ArrivalTime)), DateTimeKind.Unspecified);

                // Handle case where arrival time is on the next day (e.g., departure 10 PM, arrival 2 AM)
                if (arrivalDateTime <= tripDateTime)
                {
                    arrivalDateTime = arrivalDateTime.AddDays(1);
                }

                var trip = new Trip
                {
                    ScheduleId = schedule.Id,
                    BusId = schedule.BusId,
                    RouteId = schedule.RouteId,
                    OperatorId = schedule.OperatorId,
                    DepartureTime = tripDateTime,
                    ArrivalTime = arrivalDateTime,
                    BaseFare = schedule.BaseFare,
                    Status = TripStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Trips.Add(trip);
                await context.SaveChangesAsync(cancellationToken);

                // Create TripSeats for all active seats in the bus
                var activeSeats = schedule.Bus.Seats.Where(x => x.IsActive).ToList();
                var tripSeats = activeSeats.Select(seat => new TripSeat
                {
                    TripId = trip.Id,
                    SeatId = seat.Id,
                    Status = SeatStatus.Available,
                    LockedUntil = null,
                    LockedBy = null
                }).ToList();

                context.TripSeats.AddRange(tripSeats);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    /// <summary>
    /// Check if the given date's day of week is included in the schedule's DaysOfWeek string.
    /// DaysOfWeek format: "Monday,Wednesday,Friday" or "Mon,Wed,Fri"
    /// </summary>
    private static bool IsScheduleActiveOnDayOfWeek(DateOnly date, string daysOfWeek)
    {
        var dayOfWeekName = date.DayOfWeek.ToString();
        var dayOfWeekShort = dayOfWeekName.Substring(0, 3);
        var dayOfWeekNumber = ((int)date.DayOfWeek + 6) % 7 + 1;

        // Split by comma and trim whitespace
        var activeDays = daysOfWeek
            .Split(',')
            .Select(x => x.Trim())
            .ToList();

        // Check both full name and short name (Monday or Mon)
        return activeDays.Contains(dayOfWeekName, StringComparer.OrdinalIgnoreCase) ||
             activeDays.Contains(dayOfWeekShort, StringComparer.OrdinalIgnoreCase) ||
             activeDays.Contains(dayOfWeekNumber.ToString(), StringComparer.OrdinalIgnoreCase);
    }
}
