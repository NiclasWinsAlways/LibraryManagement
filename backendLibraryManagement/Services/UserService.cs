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

        //Get all users from a list
        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role, })
                .ToListAsync();
        }

        // create user in system -role validated/normalized to allowed set
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

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateUserDto dto, bool allowRoleChange = false)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return (false, "NotFound");

            // Normalize and validate email uniqueness
            var newEmail = dto.Email.Trim() ?? user.Email;
            if (!string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == newEmail && u.Id != id);
                if (exists) return (false, "EmailExusts");
                user.Email = newEmail;
            }

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name.Trim();
            //update role when allowed
            if (!string.IsNullOrWhiteSpace(dto.Role) && allowRoleChange) { user.Role = NormalizeRole(dto.Role); }
            //update password if provided(non-empty
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = HashPassword(dto.Password);

            await _db.SaveChangesAsync();
            return (true, null);
        }
        // Normalize role: if null/empty or not in allowed list -> default to "Låner"
        private static string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return "Member";

            var trimmed = role.Trim();
            // Accept case-insensitive matches to allowed roles and return canonical casing.
            foreach (var allowed in AllowedRoles)
            {
                if (string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase))
                    return allowed;
            }

            // Unknown role -> fallback
            return "Member";
        }
        // Basic SHA-256 hashing — replace with a proper password hasher in production (e.g., ASP.NET Identity)
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public async Task<bool> VerifyPasswordAsync(string email, string password)
        {
            var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return false;
            return u.PasswordHash == HashPassword(password);
        }   

        // Optional helper: expose allowed roles (useful for UI)
        public static IReadOnlyList<string> GetAllowedRoles() => Array.AsReadOnly(AllowedRoles);
    }
}

