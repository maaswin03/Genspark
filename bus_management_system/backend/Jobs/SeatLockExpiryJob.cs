using backend.Data;
using backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend.Jobs;

/// <summary>
/// Background job that runs every 60 seconds to unlock expired reserved seats.
/// Locks expire when LockedUntil < DateTime.UtcNow.
/// </summary>
public class SeatLockExpiryJob
{
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _jobTask;

    public SeatLockExpiryJob(IServiceProvider serviceProvider)
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
        const int intervalSeconds = 60;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredLocksAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log error but continue running
                Console.WriteLine($"Error in SeatLockExpiryJob: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }

    private async Task ProcessExpiredLocksAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.UtcNow;

            // Find all reserved seats where LockedUntil has expired
            var expiredSeats = await context.TripSeats
                .Where(x => x.Status == SeatStatus.Reserved && x.LockedUntil < now)
                .ToListAsync(cancellationToken);

            if (expiredSeats.Count > 0)
            {
                foreach (var seat in expiredSeats)
                {
                    seat.Status = SeatStatus.Available;
                    seat.LockedUntil = null;
                    seat.LockedBy = null;
                }

                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
