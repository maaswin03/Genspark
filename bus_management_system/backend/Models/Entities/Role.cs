using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Entities;

[Table("roles")]
public class Role
{
	[Key]
	[Column("id")]
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	[Column("name")]
	public required string Name { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public ICollection<User> Users { get; set; } = new HashSet<User>();
}
