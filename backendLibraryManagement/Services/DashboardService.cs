using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly LibraryContext _db;
        public DashboardService(LibraryContext db) => _db = db;

        public async Task<DashboardSummaryDto> GetSummaryAsync(int dueWithinDays = 2)
        {
            if (dueWithinDays < 0) dueWithinDays = 0;
            if (dueWithinDays > 30) dueWithinDays = 30;

            var today = DateTime.UtcNow.Date;
            var dueTo = today.AddDays(dueWithinDays);

            // Books
            var totalBooks = await _db.Books.CountAsync();
            var totalCopies = await _db.Books.SumAsync(b => (int?)b.TotalCopies) ?? 0;
            var copiesAvailable = await _db.Books.SumAsync(b => (int?)b.CopiesAvailable) ?? 0;

            // Loans
            var activeLoans = await _db.Loans.CountAsync(l => l.Status == "Aktiv");
            var overdueLoans = await _db.Loans.CountAsync(l => l.Status == "Aktiv" && l.EndDate.Date < today);
            var dueSoonLoans = await _db.Loans.CountAsync(l =>
                l.Status == "Aktiv" &&
                l.EndDate.Date >= today &&
                l.EndDate.Date <= dueTo);

            // Reservations
            var activeReservations = await _db.Reservations.CountAsync(r => r.Status == "Active");
            var readyReservations = await _db.Reservations.CountAsync(r => r.Status == "Ready");

            return new DashboardSummaryDto
            {
                TotalBooks = totalBooks,
                TotalCopies = totalCopies,
                CopiesAvailable = copiesAvailable,
                ActiveLoans = activeLoans,
                DueSoonLoans = dueSoonLoans,
                OverdueLoans = overdueLoans,
                ActiveReservations = activeReservations,
                ReadyReservations = readyReservations
            };
        }

        public async Task<List<TopBookDto>> GetTopBooksAsync(int days = 30, int take = 10)
        {
            if (days < 1) days = 1;
            if (days > 365) days = 365;
            if (take < 1) take = 1;
            if (take > 50) take = 50;

            var from = DateTime.UtcNow.AddDays(-days);

            // Count loans started within period per book
            var top = await _db.Loans
                .AsNoTracking()
                .Where(l => l.StartDate >= from)
                .GroupBy(l => l.BookId)
                .Select(g => new { BookId = g.Key, LoanCount = g.Count() })
                .OrderByDescending(x => x.LoanCount)
                .Take(take)
                .ToListAsync();

            if (top.Count == 0) return new List<TopBookDto>();

            var bookIds = top.Select(x => x.BookId).ToList();

            var books = await _db.Books
                .AsNoTracking()
                .Where(b => bookIds.Contains(b.Id))
                .Select(b => new { b.Id, b.Title, b.Author })
                .ToListAsync();

            // preserve order of "top"
            var dict = books.ToDictionary(x => x.Id, x => x);

            return top.Select(x =>
            {
                dict.TryGetValue(x.BookId, out var b);
                return new TopBookDto
                {
                    BookId = x.BookId,
                    Title = b?.Title ?? "(Unknown)",
                    Author = b?.Author,
                    LoanCount = x.LoanCount
                };
            }).ToList();
        }

        public async Task<List<LoansTrendPointDto>> GetLoansTrendAsync(int days = 30)
        {
            if (days < 1) days = 1;
            if (days > 365) days = 365;

            var startDate = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var endDate = DateTime.UtcNow.Date;

            // Loans started per day
            var started = await _db.Loans
                .AsNoTracking()
                .Where(l => l.StartDate.Date >= startDate && l.StartDate.Date <= endDate)
                .GroupBy(l => l.StartDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Loans returned per day (ReturnedAt)
            var returned = await _db.Loans
                .AsNoTracking()
                .Where(l => l.ReturnedAt != null &&
                            l.ReturnedAt.Value.Date >= startDate &&
                            l.ReturnedAt.Value.Date <= endDate)
                .GroupBy(l => l.ReturnedAt!.Value.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var startedDict = started.ToDictionary(x => x.Date, x => x.Count);
            var returnedDict = returned.ToDictionary(x => x.Date, x => x.Count);

            var result = new List<LoansTrendPointDto>(days);

            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                startedDict.TryGetValue(d, out var s);
                returnedDict.TryGetValue(d, out var r);

                result.Add(new LoansTrendPointDto
                {
                    Date = d.ToString("yyyy-MM-dd"),
                    LoansStarted = s,
                    LoansReturned = r
                });
            }

            return result;
        }
    }
}
