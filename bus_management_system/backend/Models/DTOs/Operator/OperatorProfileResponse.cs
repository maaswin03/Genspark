using backend.Models.Enums;

namespace backend.Models.DTOs.Operator;

public class OperatorProfileResponse
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public required string Name { get; set; }
	public required string Email { get; set; }
	public required string Phone { get; set; }
	public required string CompanyName { get; set; }
	public required string LicenseNumber { get; set; }
	public ApprovalStatus ApprovalStatus { get; set; }
	public DateTime CreatedAt { get; set; }
}
