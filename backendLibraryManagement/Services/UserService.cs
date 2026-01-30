using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backendLibraryManagement.Services
{
    public class UserService : IUserService
    {
        private readonly LibraryContext _db;
        public UserService(LibraryContext db) => _db = db;

        // Allowed roles in system
        private static readonly string[] AllowedRoles = new[] { "Admin", "Librarian", "Member" };

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return null;

            return ToDto(u);
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    PhoneNumber = u.PhoneNumber,
                    SmsOptIn = u.SmsOptIn,
                    EmailOptIn = u.EmailOptIn,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var trimmed = email.Trim();

            return await _db.Users.FirstOrDefaultAsync(u => u.Email == trimmed);
        }

        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);

        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var role = NormalizeRole(dto.Role);

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Role = role,
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
                SmsOptIn = dto.SmsOptIn,
                EmailOptIn = dto.EmailOptIn,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return ToDto(user);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateUserDto dto, bool allowRoleChange = false)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            // Email collision check
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id);
                    if (exists) return (false, "EmailExists");
                    user.Email = newEmail;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            // Profile fields
            if (dto.PhoneNumber != null)
            {
                var p = dto.PhoneNumber.Trim();
                user.PhoneNumber = string.IsNullOrWhiteSpace(p) ? null : p;
            }

            if (dto.SmsOptIn.HasValue)
                user.SmsOptIn = dto.SmsOptIn.Value;

            if (dto.EmailOptIn.HasValue)
                user.EmailOptIn = dto.EmailOptIn.Value;

            // Role change only if allowed
            if (allowRoleChange && !string.IsNullOrWhiteSpace(dto.Role))
                user.Role = NormalizeRole(dto.Role);

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return false;

            return u.PasswordHash == HashPassword(password);
        }

        public static IReadOnlyList<string> GetAllowedRoles()
            => Array.AsReadOnly(AllowedRoles);

        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return "Member";
            var trimmed = role.Trim();

            foreach (var allowed in AllowedRoles)
            {
                if (string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase))
                    return allowed;
            }
            return "Member";
        }

        // NOTE: still simple SHA-256 (you can upgrade to PasswordHasher later)
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private static UserDto ToDto(User u) => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            PhoneNumber = u.PhoneNumber,
            SmsOptIn = u.SmsOptIn,
            EmailOptIn = u.EmailOptIn,
            CreatedAt = u.CreatedAt
        };
    }
}
