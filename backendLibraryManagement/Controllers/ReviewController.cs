using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _svc;
        public ReviewController(IReviewService svc) => _svc = svc;

        // GET: api/Review/book/3
        [HttpGet("book/{bookId:int}")]
        public async Task<IActionResult> GetForBook(int bookId)
        {
            var list = await _svc.GetForBookAsync(bookId);
            return Ok(list);
        }

        // GET: api/Review/book/3/rating
        [HttpGet("book/{bookId:int}/rating")]
        public async Task<IActionResult> Rating(int bookId)
        {
            var summary = await _svc.GetRatingSummaryAsync(bookId);
            // If you prefer strict NotFound:
            // if (summary.ReviewCount == 0 && !await _db.Books.AnyAsync(...)) return NotFound();
            return Ok(summary);
        }

        // POST: api/Review/book/3
        [HttpPost("book/{bookId:int}")]
        public async Task<IActionResult> Create(int bookId, [FromBody] CreateReviewDto dto)
        {
            var (success, error, review) = await _svc.CreateAsync(bookId, dto);
            if (!success)
            {
                return error switch
                {
                    "BookNotFound" => NotFound(new { error = "Book not found" }),
                    "UserNotFound" => NotFound(new { error = "User not found" }),
                    "InvalidRating" => BadRequest(new { error = "Rating must be between 1 and 5" }),
                    "AlreadyReviewed" => Conflict(new { error = "User already reviewed this book" }),
                    "MustBorrowBeforeReview" => Conflict(new { error = "User must borrow the book before reviewing" }),
                    _ => BadRequest(new { error })
                };
            }

            return Ok(review);
        }

        // PUT: api/Review/12
        [HttpPut("{reviewId:int}")]
        public async Task<IActionResult> Update(int reviewId, [FromBody] UpdateReviewDto dto)
        {
            var (success, error) = await _svc.UpdateAsync(reviewId, dto);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    "InvalidRating" => BadRequest(new { error = "Rating must be between 1 and 5" }),
                    _ => BadRequest(new { error })
                };
            }
            return NoContent();
        }

        // DELETE: api/Review/12
        [HttpDelete("{reviewId:int}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var ok = await _svc.DeleteAsync(reviewId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
