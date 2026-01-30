using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<FavoriteDto>> GetForUserAsync(int userId);
        Task<(bool Success, string? Error, FavoriteDto? Favorite)> AddAsync(int userId, int bookId);
        Task<bool> RemoveAsync(int userId, int bookId);
    }
}
