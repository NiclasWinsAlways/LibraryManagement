using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class ReservationService : IReservationService
    {
        private readonly LibraryContext _db;
        private readonly INotificationService _notification;

        public ReservationService(LibraryContext db, INotificationService notification)
        {
            _db = db;
            _notification = notification;
        }

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
                    Status = r.Status,
                    ExpiresAt = r.ExpiresAt
                })
                .ToListAsync();
        }

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
                Status = r.Status,
                ExpiresAt = r.ExpiresAt
            };
        }

        public async Task<(bool Success, string? Error, ReservationDto? Reservation)> CreateAsync(CreateReservationDto dto)
        {
            var book = await _db.Books.FindAsync(dto.BookId);
            if (book == null)
                return (false, "BookNotFound", null);

            // Only allow reservation when no copies available
            if (book.CopiesAvailable > 0)
                return (false, "BookAvailableLoanInstead", null);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
                return (false, "UserNotFound", null);

            // No duplicate active/ready reservation for same book+user
            var exists = await _db.Reservations.AnyAsync(r =>
                r.BookId == dto.BookId &&
                r.UserId == dto.UserId &&
                (r.Status == "Active" || r.Status == "Ready"));

            if (exists)
                return (false, "AlreadyReserved", null);

            var reservation = new Reservation
            {
                BookId = dto.BookId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Active",
                ExpiresAt = null
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            await _notification.CreateAsync(
                user.Id,
                $"You reserved '{book.Title}'. We will notify you when it's ready."
            );

            return (true, null, new ReservationDto
            {
                Id = reservation.Id,
                BookId = reservation.BookId,
                BookTitle = book.Title,
                UserId = reservation.UserId,
                UserName = user.Name,
                CreatedAt = reservation.CreatedAt,
                Status = reservation.Status,
                ExpiresAt = reservation.ExpiresAt
            });
        }

        public async Task<bool> CancelAsync(int id)
        {
            var r = await _db.Reservations
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return false;
            if (r.Status != "Active" && r.Status != "Ready") return false;

            r.Status = "Cancelled";
            r.ExpiresAt = null;

            await _db.SaveChangesAsync();

            var title = r.Book?.Title ?? "a book";
            await _notification.CreateAsync(r.UserId, $"Your reservation for '{title}' was cancelled.");

            return true;
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateReservationDto dto)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return (false, "NotFound");

            if (!string.IsNullOrWhiteSpace(dto.Status))
                r.Status = dto.Status.Trim();

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var r = await _db.Reservations.FindAsync(id);
            if (r == null) return false;

            _db.Reservations.Remove(r);
            await _db.SaveChangesAsync();
            return true;
        }

        // NEW: queue info for a book
        public async Task<object> GetQueueForBookAsync(int bookId)
        {
            var ready = await _db.Reservations
                .Where(r => r.BookId == bookId && r.Status == "Ready")
                .OrderBy(r => r.CreatedAt)
                .Select(r => new { r.Id, r.UserId, r.ExpiresAt })
                .FirstOrDefaultAsync();

            var activeCount = await _db.Reservations
                .CountAsync(r => r.BookId == bookId && r.Status == "Active");

            return new
            {
                BookId = bookId,
                Ready = ready,
                ActiveQueueCount = activeCount
            };
        }

        // NEW: expire "Ready" reservations and promote next in queue
        public async Task<int> ExpireReadyReservationsAsync()
        {
            var now = DateTime.UtcNow;

            var expired = await _db.Reservations
                .Include(r => r.Book)
                .Where(r => r.Status == "Ready" && r.ExpiresAt != null && r.ExpiresAt < now)
                .ToListAsync();

            var count = 0;

            foreach (var r in expired)
            {
                r.Status = "Expired";
                r.ExpiresAt = null;
                count++;

                var title = r.Book?.Title ?? "a book";
                await _notification.CreateAsync(r.UserId, $"Your reservation for '{title}' expired.");

                // Promote next
                var next = await _db.Reservations
                    .Where(x => x.BookId == r.BookId && x.Status == "Active")
                    .OrderBy(x => x.CreatedAt)
                    .FirstOrDefaultAsync();

                if (next != null)
                {
                    next.Status = "Ready";
                    next.ExpiresAt = DateTime.UtcNow.AddHours(48);

                    await _notification.CreateAsync(
                        next.UserId,
                        $"The book '{title}' is now ready for you. Please borrow it before {next.ExpiresAt:yyyy-MM-dd HH:mm} (UTC)."
                    );
                }
            }

            await _db.SaveChangesAsync();
            return count;
        }
    }
}
