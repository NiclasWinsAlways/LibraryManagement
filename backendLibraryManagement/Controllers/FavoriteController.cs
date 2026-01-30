using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _svc;
        public FavoriteController(IFavoriteService svc) => _svc = svc;

        // GET: api/Favorite/user/5
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var items = await _svc.GetForUserAsync(userId);
            return Ok(items);
        }

        // POST: api/Favorite/user/5/book/3
        [HttpPost("user/{userId:int}/book/{bookId:int}")]
        public async Task<IActionResult> Add(int userId, int bookId)
        {
            var (success, error, fav) = await _svc.AddAsync(userId, bookId);
            if (!success)
            {
                return error switch
                {
                    "UserNotFound" => NotFound(new { error = "User not found" }),
                    "BookNotFound" => NotFound(new { error = "Book not found" }),
                    "AlreadyExists" => Conflict(new { error = "Already in favorites" }),
                    _ => BadRequest(new { error })
                };
            }

            return Ok(fav);
        }

        // DELETE: api/Favorite/user/5/book/3
        [HttpDelete("user/{userId:int}/book/{bookId:int}")]
        public async Task<IActionResult> Remove(int userId, int bookId)
        {
            var ok = await _svc.RemoveAsync(userId, bookId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
