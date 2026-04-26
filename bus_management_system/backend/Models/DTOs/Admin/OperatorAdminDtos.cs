using System.ComponentModel.DataAnnotations;
using backend.Models.Enums;
using backend.Models.DTOs.Operator;

namespace backend.Models.DTOs.Admin;

public class OperatorDetailResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string CompanyName { get; set; }
    public required string LicenseNumber { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? BlockedReason { get; set; }
    public List<OperatorDocumentResponse> Documents { get; set; } = new();
}

public class AdminUserResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; }
}

public class PlatformFeeResponse
{
    public decimal FeeValue { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SetPlatformFeeRequest
{
    [Range(0, double.MaxValue, ErrorMessage = "Fee value cannot be negative.")]
    public decimal FeeValue { get; set; }
}
