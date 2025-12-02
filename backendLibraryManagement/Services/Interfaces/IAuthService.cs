using backendLibraryManagement.Model;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string? Token, string? Error)> AuthenticateAsync(string email, string password);
        string GenerateToken(User user);
    }
}
