using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetByIdAsync(int id);
        Task<List<UserDto>> GetAllAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<UserDto> CreateAsync(CreateUserDto dto);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateUserDto dto, bool allowRoleChange = false);
        Task<bool> VerifyPasswordAsync(string email, string password);
    }
}
