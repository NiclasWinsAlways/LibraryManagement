using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backendLibraryManagement.Services
{
    // Handles user authentication and JWT token generation.
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public AuthService(IConfiguration config, IUserService userService)
        {
            _config = config;
            _userService = userService;
        }

        // Attempts to authenticate a user based on email + password.
        // Returns Success=false if credentials are invalid.
        public async Task<(bool Success, string? Token, string? Error)> AuthenticateAsync(string email, string password)
        {
            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return (false, null, "Invalid credentials");

            var valid = await _userService.VerifyPasswordAsync(email, password);
            if (!valid) return (false, null, "Invalid credentials");

            // Generate JWT so user can authenticate future API calls.
            var token = GenerateToken(user);
            return (true, token, null);
        }

        // Generates a signed JWT token using configuration settings.
        public string GenerateToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expireMinutes = int.TryParse(jwtSection["ExpireMinutes"], out var m) ? m : 60;

            // Token expiration time
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "Member")
            };

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            // Create token object
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            // Convert token → string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}