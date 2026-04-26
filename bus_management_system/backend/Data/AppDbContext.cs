using Microsoft.EntityFrameworkCore;
using backend.Models.Entities;
using backend.Models.Enums;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Auth & Identity
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    
    // Core Business
    public DbSet<Operator> Operators { get; set; } = null!;
    public DbSet<Bus> Buses { get; set; } = null!;
    public DbSet<Seat> Seats { get; set; } = null!;
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<backend.Models.Entities.Route> Routes { get; set; } = null!;
    public DbSet<RouteStop> RouteStops { get; set; } = null!;
    public DbSet<OperatorOffice> OperatorOffices { get; set; } = null!;
    public DbSet<BoardingPoint> BoardingPoints { get; set; } = null!;
    public DbSet<DroppingPoint> DroppingPoints { get; set; } = null!;
    public DbSet<TripSchedule> TripSchedules { get; set; } = null!;
    public DbSet<Trip> Trips { get; set; } = null!;
    public DbSet<TripSeat> TripSeats { get; set; } = null!;
    public DbSet<SeatPricing> SeatPricing { get; set; } = null!;
    
    // Booking & Passengers
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<BookingSeat> BookingSeats { get; set; } = null!;
    public DbSet<Passenger> Passengers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    
    // Config & Logs
    public DbSet<PlatformFeeConfig> PlatformFeeConfigs { get; set; } = null!;
    public DbSet<OperatorDocument> OperatorDocuments { get; set; } = null!;
    public DbSet<BusChange> BusChanges { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Required to wire up Npgsql Enums on Model Creation
        modelBuilder.HasPostgresEnum<ApprovalStatus>("approval_status_enum");
        modelBuilder.HasPostgresEnum<BookingStatus>("booking_status_enum");
        modelBuilder.HasPostgresEnum<PaymentStatus>("payment_status_enum");
        modelBuilder.HasPostgresEnum<SeatStatus>("seat_status_enum");
        
        modelBuilder.HasPostgresEnum<BusType>("bus_type_enum");
        modelBuilder.HasPostgresEnum<DeckType>("deck_type_enum");
        modelBuilder.HasPostgresEnum<SeatType>("seat_type_enum");
        modelBuilder.HasPostgresEnum<Gender>("gender_enum");
        modelBuilder.HasPostgresEnum<TripStatus>("trip_status_enum");
        modelBuilder.HasPostgresEnum<BookingSeatStatus>("booking_seat_status_enum");
        modelBuilder.HasPostgresEnum<DocumentType>("document_type_enum");
        modelBuilder.HasPostgresEnum<FeeType>("fee_type_enum");
        modelBuilder.HasPostgresEnum<ChangeType>("change_type_enum");
        modelBuilder.HasPostgresEnum<NotificationChannel>("notification_channel_enum");
        modelBuilder.HasPostgresEnum<EntityType>("entity_type_enum");
        modelBuilder.HasPostgresEnum<ReferenceType>("reference_type_enum");

        modelBuilder.Entity<Operator>()
            .HasOne(o => o.User)
            .WithOne(u => u.OperatorProfile)
            .HasForeignKey<Operator>(o => o.UserId);

        modelBuilder.Entity<Operator>()
            .HasOne(o => o.ApprovedByUser)
            .WithMany(u => u.ApprovedOperators)
            .HasForeignKey(o => o.ApprovedBy);

        // Resolving Issue 3: Multiple Cascade Paths via DeleteBehavior.Restrict
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
        {
            // By default restrict cascading to prevent SQL cyclic deletion errors
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
        
        // Add Unique Constraints mapped from query.sql
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Bus>()
            .HasIndex(b => b.BusNumber)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.BookingId)
            .IsUnique();

        modelBuilder.Entity<PlatformFeeConfig>()
            .HasIndex(p => p.IsActive);

        modelBuilder.Entity<Passenger>()
            .ToTable(t => t.HasCheckConstraint("chk_passengers_age_range", "age >= 1 AND age <= 120"));

        modelBuilder.Entity<Booking>()
            .ToTable(t => t.HasCheckConstraint(
                "chk_bookings_cancel_fields",
                "status <> 'cancelled' OR (cancelled_at IS NOT NULL AND cancel_reason IS NOT NULL)"));

        modelBuilder.Entity<Payment>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("chk_payments_method", "payment_method IN ('upi', 'card', 'netbanking', 'wallet')");
                t.HasCheckConstraint("chk_payments_paid_at", "payment_status <> 'success' OR paid_at IS NOT NULL");
            });

        modelBuilder.Entity<Notification>()
            .ToTable(t => t.HasCheckConstraint(
                "chk_notifications_reference_pair",
                "(reference_type IS NULL AND reference_id IS NULL) OR (reference_type IS NOT NULL AND reference_id IS NOT NULL)"));

        modelBuilder.Entity<OperatorDocument>()
            .ToTable(t => t.HasCheckConstraint(
                "chk_op_docs_verified_pair",
                "(verified_by IS NULL AND verified_at IS NULL) OR (verified_by IS NOT NULL AND verified_at IS NOT NULL)"));
    }
}
