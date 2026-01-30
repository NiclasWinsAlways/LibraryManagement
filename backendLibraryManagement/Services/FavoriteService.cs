using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly LibraryContext _db;
        public FavoriteService(LibraryContext db) => _db = db;

        public async Task<List<FavoriteDto>> GetForUserAsync(int userId)
        {
            return await _db.Favorites
                .AsNoTracking()
                .Include(f => f.Book)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FavoriteDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    BookId = f.BookId,
                    BookTitle = f.Book != null ? f.Book.Title : null,
                    BookAuthor = f.Book != null ? f.Book.Author : null,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error, FavoriteDto? Favorite)> AddAsync(int userId, int bookId)
        {
            var userExists = await _db.Users.AnyAsync(u => u.Id == userId);
            if (!userExists) return (false, "UserNotFound", null);

            var book = await _db.Books.FindAsync(bookId);
            if (book == null) return (false, "BookNotFound", null);

            var exists = await _db.Favorites.AnyAsync(f => f.UserId == userId && f.BookId == bookId);
            if (exists) return (false, "AlreadyExists", null);

            var fav = new Favorite
            {
                UserId = userId,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Favorites.Add(fav);
            await _db.SaveChangesAsync();

            return (true, null, new FavoriteDto
            {
                Id = fav.Id,
                UserId = fav.UserId,
                BookId = fav.BookId,
                BookTitle = book.Title,
                BookAuthor = book.Author,
                CreatedAt = fav.CreatedAt
            });
        }

        public async Task<bool> RemoveAsync(int userId, int bookId)
        {
            var fav = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (fav == null) return false;

            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
