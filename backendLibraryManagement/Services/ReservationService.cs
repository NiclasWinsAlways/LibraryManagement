using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    // Handles creation, cancellation, and processing of book reservations.
    public class ReservationService: IReservationService
    {
        private readonly LibraryContext _db;
        private readonly INotificationService _notification;

        public ReservationService(LibraryContext db, INotificationService notification)
        {
            _db = db;
            _notification = notification;
        }

        // Returns all reservations, including book and user names.
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
                })
                .ToListAsync();
        }

        // Returns a single reservation by ID.
        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var r = await _db.Reservations
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

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

        // Creates a new reservation (only allowed when book is unavailable).
        public async Task<(bool Success, string? Error, ReservationDto? Reservation)> CreateAsync(CreateReservationDto dto)
        {
            var book = await _db.Books.FindAsync(dto.BookId);
            if (book == null)
                return (false, "Book not found", null);

            // Book must be fully checked-out to allow reservation
            if (book.CopiesAvailable > 0 && book.IsAvailable)
                return (false, "Book currently available — please loan instead", null);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
                return (false, "User not found", null);

            // Prevent duplicate active reservations
            var exists = await _db.Reservations
                .AnyAsync(r =>
                    r.BookId == dto.BookId &&
                    r.UserId == dto.UserId &&
                    r.Status == "Active");

            if (exists)
                return (false, "You already have an active reservation for this book", null);

            var reservation = new Reservation
            {
                BookId = dto.BookId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            // Send confirmation notification
            await _notification.CreateAsync(
                user.Id,
                $"You reserved '{book.Title}'. We will notify you when it's available for loan."
            );

            return (true, null, new ReservationDto
            {
                Id = reservation.Id,
                BookId = reservation.BookId,
                BookTitle = book.Title,
                UserId = reservation.UserId,
                UserName = user.Name,
                CreatedAt = reservation.CreatedAt,
                Status = reservation.Status
            });
        }

        // Cancels a reservation.
        public async Task<bool> CancelAsync(int id)
        {
            var r = await _db.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (r == null) return false;
            if (r.Status != "Active") return false;

            r.Status = "Cancelled";

            await _db.SaveChangesAsync();

            // Notify user cancellation happened
            var title = r.Book?.Title ?? "a book";
            await _notification.CreateAsync(
                r.UserId,
                $"Your reservation for '{title}' has been cancelled."
            );

            return true;
        }

        // Updates reservation status (e.g., Active → Fulfilled).
        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateReservationDto dto)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return (false, "NotFound");

            var oldStatus = r.Status;

            if (!string.IsNullOrWhiteSpace(dto.Status))
                r.Status = dto.Status.Trim();

            await _db.SaveChangesAsync();

            // If reservation becomes "Available", notify the user
            if (oldStatus != r.Status && r.Status == "Available")
            {
                var res = await _db.Reservations
                    .Include(x => x.Book)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (res != null)
                {
                    var title = res.Book?.Title ?? "a book";
                    await _notification.CreateAsync(
                        res.UserId,
                        $"Your reservation for '{title}' is now available for loan."
                    );
                }
            }

            return (true, null);
        }

        // Removes reservation completely.
        public async Task<bool> DeleteAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;

            _db.Reservations.Remove(r);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
