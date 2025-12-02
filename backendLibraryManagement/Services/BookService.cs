using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    // Handles book-related operations: creation, update, deletion, and queries.
    public class BookService: IBookService
    {
        private readonly LibraryContext _db;
        public BookService(LibraryContext db) => _db = db;

        // Returns all books projected into DTO form.
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
                    IsAvailable = b.IsAvailable,
                    CopiesAvailable = b.CopiesAvailable,
                }).ToListAsync();
        }

        // Returns a single book by ID.
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
                IsAvailable = b.IsAvailable,
                CopiesAvailable = b.CopiesAvailable,
            };
        }

        // Creates a new book.
        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            var book = new Book
            {
                Title = dto.Title,
                Genre = dto.Genre,
                Author = dto.Author,
                ISBN = dto.ISBN,
                CopiesAvailable = 1, // Default copy count
                IsAvailable = true
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
                IsAvailable = book.IsAvailable,
                CopiesAvailable = book.CopiesAvailable,
            };
        }

        // Updates book details.
        public async Task<bool> UpdateAsync(int id, UpdateBookDto dto)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null) return false;
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.CopiesAvailable = dto.CopiesAvailable;
            book.IsAvailable = dto.IsAvailable;
            await _db.SaveChangesAsync();
            return true;
        }

        // Deletes a book permanently.
        public async Task<bool> DeleteAsync(int id)
        {
            var Book = await _db.Books.FindAsync(id);
            if (Book == null) return false;
            _db.Books.Remove(Book);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}