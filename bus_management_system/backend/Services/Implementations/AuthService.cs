using backend.Data;
using backend.Helpers;
using backend.Models.DTOs.Auth;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using backend.Models.Enums;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwt;

        public AuthService(AppDbContext context, JwtHelper jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        public async Task<AuthResponse?> Register(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (existingUser != null)
                return null;

            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name == "user");

            if (role == null)
                throw new InvalidOperationException("Default user role is missing.");

            var user = new User
            {
                Name = request.Name.Trim(),
                Email = email,
                Phone = request.Phone.Trim(),
                Password = PasswordHasher.Hash(request.Password),
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user = await _context.Users
                .Include(x => x.Role)
                .FirstAsync(x => x.Id == user.Id);

            return CreateResponse(user);
        }

        public async Task<AuthResponse?> Login(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return null;

            var isValid = PasswordHasher.Verify(request.Password, user.Password);

            if (!isValid)
                return null;

            if (user.Role.Name.ToLower() == "operator")
            {
                var op = await _context.Operators
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (op == null || op.ApprovalStatus != ApprovalStatus.Approved)
                    return null;
            }

            return CreateResponse(user);
        }

        public async Task<OperatorRegisterResponse?> OperatorRegister(OperatorRegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (existingUser != null)
                return null;

            var role = await _context.Roles
                .FirstOrDefaultAsync(x => x.Name.ToLower() == "operator");

            if (role == null)
                throw new InvalidOperationException("Operator role is missing.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    Name = request.Name.Trim(),
                    Email = email,
                    Phone = request.Phone.Trim(),
                    Password = PasswordHasher.Hash(request.Password),
                    RoleId = role.Id
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var operatorEntity = new Operator
                {
                    UserId = user.Id,
                    CompanyName = request.CompanyName.Trim(),
                    LicenseNumber = request.LicenseNumber.Trim(),
                    ApprovalStatus = ApprovalStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Operators.Add(operatorEntity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new OperatorRegisterResponse();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        
        private AuthResponse CreateResponse(User user)
        {
            var token = _jwt.GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role.Name
            };
        }
    }
}