using backend.Models.DTOs.Admin;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocationResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _locationService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
