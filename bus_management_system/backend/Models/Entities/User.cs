using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    [Column("email")]
    public required string Email { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("password")]
    public required string Password { get; set; }

    [Required]
    [MaxLength(20)]
    [Phone]
    [Column("phone")]
    public required string Phone { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;

    public Operator? OperatorProfile { get; set; }

    public ICollection<Operator> ApprovedOperators { get; set; } = new HashSet<Operator>();
    public ICollection<TripSeat> SeatLocks { get; set; } = new HashSet<TripSeat>();
    public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
}
