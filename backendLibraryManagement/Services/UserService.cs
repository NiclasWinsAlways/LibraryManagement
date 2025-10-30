using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backendLibraryManagement.Services
{
    public class UserService
    {
        private readonly LibraryContext _db;
        public UserService(LibraryContext db) => _db = db;
        private static readonly string[] AllowedRoles = new[] { "Admin", "Librarian", "Member" };

        //get user by id
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return null;
            return new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role };
        }

        //Get all users
        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role })
                .ToListAsync();
        }

        // Get user entity by email (for auth)
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Email == email.Trim());
        }

        // Check if email exists
        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);

        // create user in system - role validated/normalized to allowed set
        public async Task<UserDto> CreateAsync(CreateUserDto dto)
        {
            var role = NormalizeRole(dto.Role);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = role
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return new UserDto { Id = user.Id, Name = user.Name, Email = user.Email, Role = user.Role };
        }

        // Update user (existing method signature preserved elsewhere)
        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateUserDto dto, bool allowRoleChange = false)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            var newEmail = dto.Email?.Trim() ?? user.Email;
            if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id);
                if (exists) return (false, "EmailExists");
                user.Email = newEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Role) && allowRoleChange)
                user.Role = NormalizeRole(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            await _db.SaveChangesAsync();
            return (true, null);
        }

        // Verify password
        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return false;
            return u.PasswordHash == HashPassword(password);
        }

        // Normalize role
        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Member";

            var trimmed = role.Trim();
            foreach (var allowed in AllowedRoles)
            {
                if (string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase))
                    return allowed;
            }
            return "Member";
        }

        // Basic SHA-256 hashing — replace with ASP.NET Identity in production
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // Optional helper: expose allowed roles
        public static IReadOnlyList<string> GetAllowedRoles() => Array.AsReadOnly(AllowedRoles);
    }
}