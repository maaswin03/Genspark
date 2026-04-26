using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Auth;

public class OperatorRegisterResponse
{
    public string Message { get; set; } = 
        "Operator registered successfully. Await admin approval.";
}

public class RegisterRequest
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
}
