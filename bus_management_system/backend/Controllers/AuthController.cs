using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models.DTOs.Auth;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _service.Login(request);

            if (result == null)
                return Unauthorized("Invalid credentials");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
        {
            var result = await _service.Register(request);

            if (result == null)
                return Conflict("User already exists");

            return Ok(result);
        }

        [HttpPost("operator/register")]
        public async Task<ActionResult<AuthResponse>> OperatorRegister([FromBody] OperatorRegisterRequest request)
        {
            var result = await _service.OperatorRegister(request);

            if (result == null)
                return Conflict("Operator already exists");

            return Ok(result);
        }
    }
}