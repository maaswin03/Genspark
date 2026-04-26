using System.ComponentModel.DataAnnotations;

namespace backend.Models.DTOs.Admin;

public class CreateLocationRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    public required string City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
    public required string State { get; set; }
}

public class UpdateLocationRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "City is required.")]
    [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    public required string City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
    public required string State { get; set; }
}

public class LocationResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
}
