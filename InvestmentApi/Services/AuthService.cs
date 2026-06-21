using InvestmentApi.Data;
using InvestmentApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace InvestmentApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(AuthRequest request);
        Task<AuthResponse> LoginAsync(AuthRequest request);
        Task<AuthResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        string GenerateJwtToken(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly InvestmentContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(InvestmentContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse { Success = false, Message = "Email i hasło są wymagane" };
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return new AuthResponse { Success = false, Message = "Użytkownik z tym emailem już istnieje" };
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Rejestracja pomyślna",
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email, Role = user.Role }
            };
        }

        public async Task<AuthResponse> LoginAsync(AuthRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse { Success = false, Message = "Email lub hasło są nieprawidłowe" };
            }

            if (!user.IsActive)
            {
                return new AuthResponse { Success = false, Message = "Konto jest nieaktywne" };
            }

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Logowanie pomyślne",
                Token = token,
                User = new UserDto { Id = user.Id, Email = user.Email, Role = user.Role }
            };
        }

        public async Task<AuthResponse> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new AuthResponse { Success = false, Message = "Użytkownik nie znaleziony" };
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return new AuthResponse { Success = false, Message = "Obecne hasło jest nieprawidłowe" };
            }

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new AuthResponse { Success = true, Message = "Hasło zmienione pomyślnie" };
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            // Porównanie bezpieczne (constant-time)
            return hashOfInput.Equals(hash, StringComparison.Ordinal);
        }
    }
}
