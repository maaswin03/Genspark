using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Admin;

public class ApproveOperatorRequest
{
	[MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
	public string? Note { get; set; }
}
