using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class ReservationService
    {
        private readonly LibraryContext _db;
        public ReservationService(LibraryContext db) => _db = db;

        public async Task<List<ReservationDto>> GetAllAsync()
        {
            return await _db.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsNoTracking()
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book != null ? r.Book.Title : null,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : null,
                    CreatedAt = r.CreatedAt,
                    Status = r.Status
                }).ToListAsync();
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var r = await _db.Reservations.Include(x => x.Book).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return null;
            return new ReservationDto
            {
                Id = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book?.Title,
                UserId = r.UserId,
                UserName = r.User?.Name,
                CreatedAt = r.CreatedAt,
                Status = r.Status
            };
        }

        public async Task<(bool Success, string? Error, ReservationDto? Reservation)> CreateAsync(CreateReservationDto dto)
        {
            var book = await _db.Books.FindAsync(dto.BookId);
            if (book == null) return (false, "Book not found", null);

            // Only allow reservation when book has no available copies
            if (book.CopiesAvailable > 0 && book.IsAvailable) return (false, "Book currently available — please loan instead", null);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return (false, "User not found", null);

            // Prevent duplicate active reservation by same user for same book
            var exists = await _db.Reservations.AnyAsync(r => r.BookId == dto.BookId && r.UserId == dto.UserId && r.Status == "Active");
            if (exists) return (false, "You already have an active reservation for this book", null);

            var reservation = new Reservation
            {
                BookId = dto.BookId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            var notifsvc = new NotificationService(_db);
            await notifsvc.CreateAsync(
                user.Id,
                $"You reserved '{book.Title}'. We will notify you when it's available for loan."
                );

            var result = new ReservationDto
            {
                Id = reservation.Id,
                BookId = reservation.BookId,
                BookTitle = book.Title,
                UserId = reservation.UserId,
                UserName = user.Name,
                CreatedAt = reservation.CreatedAt,
                Status = reservation.Status
            };

            return (true, null, result);
        }

        public async Task<bool> CancelAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;
            if (r.Status != "Active") return false;
            r.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return true;
        }
    }
}