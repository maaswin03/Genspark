using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Models.Enums;

namespace backend.Models.Entities;

[Table("passengers")]
public class Passenger
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("booking_seat_id")]
    public int BookingSeatId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("gender")]
    public Gender Gender { get; set; }

    [ForeignKey(nameof(BookingSeatId))]
    public BookingSeat BookingSeat { get; set; } = null!;
}
