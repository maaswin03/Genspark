using backend.Models.DTOs.Auth;

namespace backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> Login(LoginRequest request);
        Task<AuthResponse?> Register(RegisterRequest request);
        Task<OperatorRegisterResponse?> OperatorRegister(OperatorRegisterRequest request);
    }
}