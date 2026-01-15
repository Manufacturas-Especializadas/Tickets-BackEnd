using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tickets.Dtos;
using Tickets.Models;

namespace Tickets.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHaser;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHaser = new PasswordHasher<User>();
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if(user == null)
            {
                return false;
            }

            user.RefreshToken = null!;
            user.RefreshTokenExpiryTime = null;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                        .Include(u => u.Rol)
                        .FirstOrDefaultAsync(u => u.PayRollNumber == request.PayRollNumber);

            if (user == null) return null;

            var result = _passwordHaser.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed) return null;

            return await CreateTokenResponseAsync(user);
        }

        public async Task<User?> RegisterAsync(RegisterRequestDto request)
        {
            if(await _context.Users.AnyAsync(u => u.PayRollNumber == request.PayRollNumber))
            {
                return null;
            }

            var role = await _context.Roles
                                .FirstOrDefaultAsync(r => r.Name == request.RoleName);

            if(role == null)
            {
                throw new ArgumentException($"El rol {request.RoleName} no existe");
            }

            var user = new User
            {
                Name = request.Name,
                PayRollNumber = request.PayRollNumber,
                RolId = role.Id,
                PasswordHash = _passwordHaser.HashPassword(null!, request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users
                            .Include(u => u.Rol)
                            .FirstOrDefaultAsync(u => 
                                u.RefreshToken == refreshToken && 
                                u.RefreshTokenExpiryTime > DateTime.UtcNow
                            );

            if (user == null) return null;

            return await CreateTokenResponseAsync(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponseAsync(User user)
        {
            var accessToken = CreateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddDays(5)
            };
        }

        private string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Rol.Name),
                new Claim("PayRollNumber", user.PayRollNumber.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }
    }
}