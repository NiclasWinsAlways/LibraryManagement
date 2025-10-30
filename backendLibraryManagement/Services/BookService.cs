using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class BookService
    {
        private readonly LibraryContext _db;
        public BookService(LibraryContext db)=>_db = db;

        //get all book from a list
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

        //get one book by id
        public async Task<BookDto?>GetByIdAsync(int id)
        {
            var b=await _db.Books.FindAsync(id);
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

        //create book in system
        public async Task<BookDto> CreateAsync(CreateBookDto dto)
        {
            var Book = new Book
            {
                Title = dto.Title,
                Genre = dto.Genre,
                Author = dto.Author,
                ISBN = dto.ISBN,
                IsAvailable = true,
                //CopiesAvailable implemeten wen done wit other work
            };
            _db.Books.Add(Book);
            await _db.SaveChangesAsync();
            return new BookDto
            {
                Id = Book.Id,
                Title = Book.Title,
                Genre = Book.Genre,
                Author = Book.Author,
                ISBN = Book.ISBN,
                IsAvailable = Book.IsAvailable,
                CopiesAvailable = Book.CopiesAvailable,
            };
        }

        //update book
        public async Task<bool>UpdateAsync(int id, UpdateBookDto dto)
        {
            var book=await _db.Books.FindAsync(id);
            if (book == null) return false;
            book.Title = dto.Title;
            book.Author = dto.Author;
            book.CopiesAvailable = dto.CopiesAvailable;
            book.IsAvailable = dto.IsAvailable;
            await _db.SaveChangesAsync();
            return true;
        }

        //delete book
        public async Task<bool>DeleteAsync(int id)
        {
            var Book=await _db.Books.FindAsync(id);
            if (Book == null) return false;
            _db.Books.Remove(Book);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
