using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class LoanService : ILoanService
    {
        private readonly LibraryContext _db;
        private readonly INotificationService _notification;

        public LoanService(LibraryContext db, INotificationService notification)
            => (_db, _notification) = (db, notification);

        public async Task<List<LoanDto>> GetAllAsync()
        {
            return await _db.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .AsNoTracking()
                .Select(l => new LoanDto
                {
                    Id = l.Id,
                    BookId = l.BookId,
                    BookTitle = l.Book != null ? l.Book.Title : null,
                    UserId = l.UserId,
                    UserName = l.User != null ? l.User.Name : null,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    ReturnedAt = l.ReturnedAt,
                    ExtendedCount = l.ExtendedCount,
                    Status = l.Status
                }).ToListAsync();
        }

        public async Task<LoanDto?> GetByIdAsync(int id)
        {
            var l = await _db.Loans
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (l == null) return null;

            return new LoanDto
            {
                Id = l.Id,
                BookId = l.BookId,
                BookTitle = l.Book?.Title,
                UserId = l.UserId,
                UserName = l.User?.Name,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                ReturnedAt = l.ReturnedAt,
                ExtendedCount = l.ExtendedCount,
                Status = l.Status
            };
        }

        public async Task<(bool Success, string? Error, LoanDto? Loan)> CreateAsync(CreateLoanDto dto)
        {
            var book = await _db.Books.FindAsync(dto.BookId);
            if (book == null) return (false, "BookNotFound", null);

            if (book.CopiesAvailable <= 0) return (false, "BookNotAvailable", null);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return (false, "UserNotFound", null);

            // BLOCK: Ready reservation exists for another user
            var readyRes = await _db.Reservations
                .Where(r => r.BookId == dto.BookId && r.Status == "Ready")
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (readyRes != null && readyRes.UserId != dto.UserId)
                return (false, "ReservedForAnotherUser", null);

            // BLOCK: Active queue exists and first in line is not this user
            var firstActive = await _db.Reservations
                .Where(r => r.BookId == dto.BookId && r.Status == "Active")
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (firstActive != null && firstActive.UserId != dto.UserId)
                return (false, "QueueExists", null);

            var loan = new Loan
            {
                BookId = dto.BookId,
                UserId = dto.UserId,
                StartDate = DateTime.UtcNow,
                EndDate = dto.EndDate,
                Status = "Aktiv",
                ReturnedAt = null,
                ExtendedCount = 0
            };

            book.CopiesAvailable -= 1;

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync();

            // If user had Ready reservation, fulfill it
            if (readyRes != null && readyRes.UserId == dto.UserId)
            {
                readyRes.Status = "Fulfilled";
                readyRes.ExpiresAt = null;
                await _db.SaveChangesAsync();
            }

            var result = new LoanDto
            {
                Id = loan.Id,
                BookId = loan.BookId,
                BookTitle = book.Title,
                UserId = loan.UserId,
                UserName = user.Name,
                StartDate = loan.StartDate,
                EndDate = loan.EndDate,
                ReturnedAt = loan.ReturnedAt,
                ExtendedCount = loan.ExtendedCount,
                Status = loan.Status
            };

            return (true, null, result);
        }

        public async Task<bool> ReturnLoanAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null) return false;
            if (loan.Status == "Afleveret") return false;

            loan.Status = "Afleveret";
            loan.ReturnedAt = DateTime.UtcNow;

            var book = loan.Book!;
            book.CopiesAvailable++;

            await _db.SaveChangesAsync();

            // If someone is in queue -> set next to Ready
            var next = await _db.Reservations
                .Where(r => r.BookId == book.Id && r.Status == "Active")
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (next != null)
            {
                next.Status = "Ready";
                next.ExpiresAt = DateTime.UtcNow.AddHours(48);

                await _db.SaveChangesAsync();

                await _notification.CreateAsync(
                    next.UserId,
                    $"The book '{book.Title}' you reserved is now ready. Please borrow it before {next.ExpiresAt:yyyy-MM-dd HH:mm} (UTC)."
                );
            }

            return true;
        }

        public async Task<(bool Success, string? Error, LoanDto? Loan)> ExtendLoanAsync(int loanId, int days)
        {
            if (days <= 0 || days > 30) return (false, "InvalidDays", null);

            var loan = await _db.Loans
                .Include(l => l.Book)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null) return (false, "NotFound", null);
            if (loan.Status != "Aktiv") return (false, "LoanNotActive", null);
            if (loan.EndDate < DateTime.UtcNow) return (false, "LoanOverdue", null);

            const int maxExtensions = 2;
            if (loan.ExtendedCount >= maxExtensions) return (false, "MaxExtensionsReached", null);

            // BLOCK: If reserved/queued by others, extension not allowed
            var hasQueue = await _db.Reservations.AnyAsync(r =>
                r.BookId == loan.BookId &&
                (r.Status == "Active" || r.Status == "Ready") &&
                r.UserId != loan.UserId);

            if (hasQueue) return (false, "ReservedByOthers", null);

            loan.EndDate = loan.EndDate.AddDays(days);
            loan.ExtendedCount += 1;

            await _db.SaveChangesAsync();

            return (true, null, new LoanDto
            {
                Id = loan.Id,
                BookId = loan.BookId,
                BookTitle = loan.Book?.Title,
                UserId = loan.UserId,
                UserName = loan.User?.Name,
                StartDate = loan.StartDate,
                EndDate = loan.EndDate,
                ReturnedAt = loan.ReturnedAt,
                ExtendedCount = loan.ExtendedCount,
                Status = loan.Status
            });
        }

        // Keep your existing function if you still use it.
        public async Task CheckDueLoansAsync()
        {
            var now = DateTime.UtcNow;

            var soonDue = await _db.Loans
                .Include(l => l.Book)
                .Where(l => l.Status == "Aktiv" &&
                            l.EndDate <= now.AddDays(2) &&
                            l.EndDate > now)
                .ToListAsync();

            foreach (var loan in soonDue)
            {
                await _notification.CreateAsync(
                    loan.UserId,
                    $"Reminder: Your loan for '{loan.Book!.Title}' is due on {loan.EndDate:yyyy-MM-dd}."
                );

            }
        }
    }
}
