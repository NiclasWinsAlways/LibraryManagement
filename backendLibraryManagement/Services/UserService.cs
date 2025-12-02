using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backendLibraryManagement.Services
{
    // Handles CRUD operations and authentication helpers for users.
    public class UserService: IUserService
    {
        private readonly LibraryContext _db;

        public UserService(LibraryContext db) => _db = db;

        // Only these roles are allowed in the system.
        private static readonly string[] AllowedRoles = new[] { "Admin", "Librarian", "Member" };

        // Fetch a single user by ID.
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var u = await _db.Users.FindAsync(id);
            if (u == null) return null;

            return new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            };
        }

        // Fetch all users.
        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
        }

        // Fetch full user model for authentication.
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email.Trim());
        }

        // Checks if an email is already registered.
        public async Task<bool> EmailExistsAsync(string email)
            => await _db.Users.AnyAsync(u => u.Email == email);

        // Creates a user, normalizing role and hashing password.
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

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        // Updates a user, optionally allowing role changes if authorized.
        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateUserDto dto, bool allowRoleChange = false)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            // Handle email changes (with collision check)
            var newEmail = dto.Email?.Trim() ?? user.Email;

            if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id);
                if (exists) return (false, "EmailExists");

                user.Email = newEmail;
            }

            // Optional fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Role) && allowRoleChange)
                user.Role = NormalizeRole(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            await _db.SaveChangesAsync();
            return (true, null);
        }

        // Validates password by hashing and comparing.
        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return false;

            return u.PasswordHash == HashPassword(password);
        }

        // Normalizes roles and ensures they match allowed values.
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

        // SHA-256 password hashing (simple example — use ASP.NET Identity in real systems).
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        // Exposes allowed roles for UI / client display.
        public static IReadOnlyList<string> GetAllowedRoles()
            => Array.AsReadOnly(AllowedRoles);
    }
}
