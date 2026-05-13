using Microsoft.EntityFrameworkCore;
using NotificationModelLibrary;

namespace NotificationDALLibrary.Context
{
    public class NotificationSystemContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=notification;Username=aswin;Password=Nopassword123");
        }

        public DbSet<User> users { get; set; }

        public DbSet<Notification> notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(u =>
            {
                u.HasKey(u => u.UserId); //setting userid as primary key

                u.HasIndex(u => u.Email).IsUnique(); //email should be Unique

                u.HasIndex(u => u.PhoneNumber).IsUnique(); //PhoneNumber should be Unique

                u.Property(u => u.CreatedAt).HasColumnType("timestamp without time zone"); //fixing timezone issue
            });

            modelBuilder.Entity<Notification>(n =>
            {
                n.HasKey(n => n.MessageId);

                n.Property(n => n.SendedAt).HasColumnType("timestamp without time zone"); // fixing time zone issue

                n.HasOne(n => n.User)
                .WithMany(n => n.Notifications)
                .HasForeignKey(n => n.ReceiverId) //setting up ForeignKey
                .HasConstraintName("FK_RECEIVER_ID")
                .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}