using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Admin;

public class RejectOperatorRequest
{
	[Required(ErrorMessage = "Reason is required.")]
	[MaxLength(1000, ErrorMessage = "Reason cannot exceed 1000 characters.")]
	public required string Reason { get; set; }
}
