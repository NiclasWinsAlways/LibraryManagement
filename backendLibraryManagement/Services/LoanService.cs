    using Microsoft.EntityFrameworkCore;
    using backendLibraryManagement.Data;
    using backendLibraryManagement.Dto;
    using backendLibraryManagement.Model;

    namespace backendLibraryManagement.Services
    {
        public class LoanService
        {
            private readonly LibraryContext _db;
        private readonly NotificationService _notification;
        public LoanService(LibraryContext db, NotificationService notification) => (_db,_notification) = (db, notification);

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
                        Status = l.Status
                    }).ToListAsync();
            }

            public async Task<LoanDto?> GetByIdAsync(int id)
            {
                var l = await _db.Loans.Include(x => x.Book).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
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
                    Status = l.Status
                };
            }

            public async Task<(bool Success, string? Error, LoanDto? Loan)> CreateAsync(CreateLoanDto dto)
            {
                var book = await _db.Books.FindAsync(dto.BookId);
                if (book == null) return (false, "Book not found", null);
                if (book.CopiesAvailable <= 0 || !book.IsAvailable) return (false, "Book not available; consider reserving it", null);

                var user = await _db.Users.FindAsync(dto.UserId);
                if (user == null) return (false, "User not found", null);

                var loan = new Loan
                {
                    BookId = dto.BookId,
                    UserId = dto.UserId,
                    StartDate = DateTime.UtcNow,
                    EndDate = dto.EndDate,
                    Status = "Aktiv"
                };

                // Business rule: decrement available copies
                book.CopiesAvailable -= 1;
                if (book.CopiesAvailable <= 0) book.IsAvailable = false;

                _db.Loans.Add(loan);
                await _db.SaveChangesAsync();

                var result = new LoanDto
                {
                    Id = loan.Id,
                    BookId = loan.BookId,
                    BookTitle = book.Title,
                    UserId = loan.UserId,
                    UserName = user.Name,
                    StartDate = loan.StartDate,
                    EndDate = loan.EndDate,
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
                loan.EndDate = DateTime.UtcNow;

                var book = loan.Book!;
                book.CopiesAvailable++;
                book.IsAvailable = book.CopiesAvailable > 0;

                await _db.SaveChangesAsync();

                // ✅ Notify next user with reservation
                var nextReservation = await _db.Reservations
                    .Where(r => r.BookId == book.Id && r.Status == "Active")
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                if (nextReservation != null)
                {
                    nextReservation.Status = "Fulfilled";
                    await _db.SaveChangesAsync();
                    //added this 
                    await _notification.CreateAsync(
                        nextReservation.UserId,
                        $"The book '{book.Title}' you reserved is now available for loan."
                    );
            }
                return true;
            }
        public async Task CheckDueLoansAsync()
        {
            var now = DateTime.UtcNow;

            var soonDue = await _db.Loans
                .Include(l => l.Book)
                .Where(l => l.Status == "Aktiv" &&
                            l.EndDate <= now.AddDays(2) &&
                            l.EndDate > now)
                .ToListAsync();
            //added this 
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