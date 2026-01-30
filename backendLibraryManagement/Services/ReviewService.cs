using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class ReviewService : IReviewService
    {
        private readonly LibraryContext _db;
        public ReviewService(LibraryContext db) => _db = db;

        public async Task<List<ReviewDto>> GetForBookAsync(int bookId)
        {
            return await _db.Reviews
                .AsNoTracking()
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.BookId == bookId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    BookId = r.BookId,
                    BookTitle = r.Book != null ? r.Book.Title : null,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.Name : null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<BookRatingSummaryDto> GetRatingSummaryAsync(int bookId)
        {
            var exists = await _db.Books.AnyAsync(b => b.Id == bookId);
            if (!exists)
            {
                // return empty summary for non-existing book (controller may choose NotFound)
                return new BookRatingSummaryDto { BookId = bookId, AverageRating = 0, ReviewCount = 0 };
            }

            var query = _db.Reviews.AsNoTracking().Where(r => r.BookId == bookId);

            var count = await query.CountAsync();
            if (count == 0)
            {
                return new BookRatingSummaryDto { BookId = bookId, AverageRating = 0, ReviewCount = 0 };
            }

            // EF will translate Average for int -> double
            var avg = await query.AverageAsync(r => (double)r.Rating);

            return new BookRatingSummaryDto
            {
                BookId = bookId,
                AverageRating = Math.Round(avg, 2),
                ReviewCount = count
            };
        }

        public async Task<(bool Success, string? Error, ReviewDto? Review)> CreateAsync(int bookId, CreateReviewDto dto)
        {
            var book = await _db.Books.FindAsync(bookId);
            if (book == null) return (false, "BookNotFound", null);

            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return (false, "UserNotFound", null);

            if (dto.Rating < 1 || dto.Rating > 5) return (false, "InvalidRating", null);

            // Unique: one review per (book,user)
            var exists = await _db.Reviews.AnyAsync(r => r.BookId == bookId && r.UserId == dto.UserId);
            if (exists) return (false, "AlreadyReviewed", null);

            // Optional rule: only allow if user has borrowed the book before:
            // var hasLoan = await _db.Loans.AnyAsync(l => l.BookId == bookId && l.UserId == dto.UserId);
            // if (!hasLoan) return (false, "MustBorrowBeforeReview", null);

            var review = new Review
            {
                BookId = bookId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            return (true, null, new ReviewDto
            {
                Id = review.Id,
                BookId = review.BookId,
                BookTitle = book.Title,
                UserId = review.UserId,
                UserName = user.Name,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            });
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int reviewId, UpdateReviewDto dto)
        {
            var r = await _db.Reviews
                .Include(x => x.Book)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == reviewId);

            if (r == null) return (false, "NotFound");

            if (dto.Rating.HasValue)
            {
                if (dto.Rating.Value < 1 || dto.Rating.Value > 5) return (false, "InvalidRating");
                r.Rating = dto.Rating.Value;
            }

            if (dto.Comment != null)
            {
                var trimmed = dto.Comment.Trim();
                r.Comment = string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
            }

            r.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int reviewId)
        {
            var r = await _db.Reviews.FindAsync(reviewId);
            if (r == null) return false;

            _db.Reviews.Remove(r);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
