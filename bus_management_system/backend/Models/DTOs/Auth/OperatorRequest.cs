using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Auth;

public class OperatorRegisterRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(255)]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [MaxLength(20)]
    public required string Phone { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
    public required string CompanyName { get; set; }

    [Required(ErrorMessage = "License number is required.")]
    [MaxLength(100, ErrorMessage = "License number cannot exceed 100 characters.")]
    public required string LicenseNumber { get; set; }
}