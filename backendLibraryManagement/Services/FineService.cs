using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backendLibraryManagement.Services
{
    public class FineService : IFineService
    {
        private readonly LibraryContext _db;
        private readonly FineOptions _opt;

        public FineService(LibraryContext db, IOptions<FineOptions> opt)
        {
            _db = db;
            _opt = opt.Value;
        }

        public async Task<List<FineDto>> GetFinesForUserAsync(int userId)
        {
            return await _db.Fines
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FineDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    LoanId = f.LoanId,
                    Amount = f.Amount,
                    Reason = f.Reason,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    PaidAt = f.PaidAt
                })
                .ToListAsync();
        }

        public async Task<int> RunOverdueFineScanAsync()
        {
            if (!_opt.Enabled) return 0;

            var today = DateTime.UtcNow.Date;

            // Find active overdue loans
            var overdueLoans = await _db.Loans
                .AsNoTracking()
                .Where(l => l.Status == "Aktiv" && l.EndDate.Date < today)
                .Select(l => new { l.Id, l.UserId, l.EndDate })
                .ToListAsync();

            if (overdueLoans.Count == 0) return 0;

            var loanIds = overdueLoans.Select(x => x.Id).ToList();

            // Existing unpaid fines for those loans
            var existingUnpaid = await _db.Fines
                .Where(f => loanIds.Contains(f.LoanId) && f.Status == "Unpaid")
                .ToListAsync();

            var existingMap = existingUnpaid.ToDictionary(f => f.LoanId, f => f);

            var changed = 0;

            foreach (var loan in overdueLoans)
            {
                var daysLate = (today - loan.EndDate.Date).Days;
                if (daysLate <= 0) continue;

                var amount = daysLate * _opt.DailyRate;
                if (amount > _opt.MaxFine) amount = _opt.MaxFine;
                if (amount < 0) amount = 0;

                if (existingMap.TryGetValue(loan.Id, out var fine))
                {
                    // Update amount as days increase (MVP)
                    if (fine.Amount != amount)
                    {
                        fine.Amount = amount;
                        changed++;
                    }
                }
                else
                {
                    var newFine = new Fine
                    {
                        UserId = loan.UserId,
                        LoanId = loan.Id,
                        Amount = amount,
                        Reason = $"Overdue loan ({daysLate} day(s) late)",
                        Status = "Unpaid",
                        CreatedAt = DateTime.UtcNow
                    };

                    _db.Fines.Add(newFine);
                    changed++;
                }
            }

            if (changed > 0)
                await _db.SaveChangesAsync();

            return changed;
        }

        public async Task<(bool Success, string? Error, ReceiptDto? Receipt)> PayFineAsync(int fineId, int userId)
        {
            var fine = await _db.Fines
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == fineId);

            if (fine == null) return (false, "NotFound", null);
            if (fine.UserId != userId) return (false, "Forbidden", null);

            if (fine.Status == "Paid")
            {
                // Return existing receipt if already paid
                var existing = await _db.Receipts.AsNoTracking().FirstOrDefaultAsync(r => r.FineId == fine.Id);
                if (existing != null)
                {
                    return (true, null, new ReceiptDto
                    {
                        Id = existing.Id,
                        UserId = existing.UserId,
                        FineId = existing.FineId,
                        ReceiptNumber = existing.ReceiptNumber,
                        Amount = existing.Amount,
                        IssuedAt = existing.IssuedAt
                    });
                }
                return (false, "AlreadyPaid", null);
            }

            if (fine.Status != "Unpaid") return (false, "NotPayable", null);

            // Mark paid
            fine.Status = "Paid";
            fine.PaidAt = DateTime.UtcNow;

            // Create receipt
            var receipt = new Receipt
            {
                UserId = fine.UserId,
                FineId = fine.Id,
                Amount = fine.Amount,
                ReceiptNumber = GenerateReceiptNumber(),
                IssuedAt = DateTime.UtcNow
            };

            _db.Receipts.Add(receipt);
            await _db.SaveChangesAsync();

            return (true, null, new ReceiptDto
            {
                Id = receipt.Id,
                UserId = receipt.UserId,
                FineId = receipt.FineId,
                ReceiptNumber = receipt.ReceiptNumber,
                Amount = receipt.Amount,
                IssuedAt = receipt.IssuedAt
            });
        }

        public async Task<ReceiptDto?> GetReceiptAsync(int receiptId)
        {
            var r = await _db.Receipts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == receiptId);
            if (r == null) return null;

            return new ReceiptDto
            {
                Id = r.Id,
                UserId = r.UserId,
                FineId = r.FineId,
                ReceiptNumber = r.ReceiptNumber,
                Amount = r.Amount,
                IssuedAt = r.IssuedAt
            };
        }

        public async Task<List<ReceiptDto>> GetReceiptsForUserAsync(int userId)
        {
            return await _db.Receipts
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.IssuedAt)
                .Select(r => new ReceiptDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    FineId = r.FineId,
                    ReceiptNumber = r.ReceiptNumber,
                    Amount = r.Amount,
                    IssuedAt = r.IssuedAt
                })
                .ToListAsync();
        }

        private static string GenerateReceiptNumber()
        {
            // Unique enough for MVP
            return $"R-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}
