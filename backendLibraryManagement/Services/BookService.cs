using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class BookService : IBookService
    {
        private readonly LibraryContext _db;
        public BookService(LibraryContext db) => _db = db;

        public async Task<List<BookDto>> GetAllAsync()
        {
            return await _db.Books
                .AsNoTracking()
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    TotalCopies = b.TotalCopies,
                    CopiesAvailable = b.CopiesAvailable,
                    IsAvailable = b.CopiesAvailable > 0
                }).ToListAsync();
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var b = await _db.Books.FindAsync(id);
            if (b == null) return null;

            return new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Genre = b.Genre,
                Author = b.Author,
                ISBN = b.ISBN,
                TotalCopies = b.TotalCopies,
                CopiesAvailable = b.CopiesAvailable,
                IsAvailable = b.CopiesAvailable > 0
            };
        }

        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            var total = dto.TotalCopies < 0 ? 0 : dto.TotalCopies;

            var book = new Book
            {
                Title = dto.Title,
                Genre = dto.Genre,
                Author = dto.Author,
                ISBN = dto.ISBN,
                TotalCopies = total,
                CopiesAvailable = total
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            return new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Genre = book.Genre,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                CopiesAvailable = book.CopiesAvailable,
                IsAvailable = book.CopiesAvailable > 0
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateBookDto dto)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return false;

            book.Title = dto.Title;
            book.Author = dto.Author;

            // Keep totals sane
            var newTotal = dto.TotalCopies < 0 ? 0 : dto.TotalCopies;
            var newAvail = dto.CopiesAvailable < 0 ? 0 : dto.CopiesAvailable;

            // If you reduce total below available, clamp available
            if (newAvail > newTotal) newAvail = newTotal;

            book.TotalCopies = newTotal;
            book.CopiesAvailable = newAvail;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return false;

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
