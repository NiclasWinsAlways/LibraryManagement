using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _svc;
        public ReservationController(IReservationService svc) => _svc = svc;

        [HttpGet("getReservations")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var res = await _svc.GetByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var (success, error, reservation) = await _svc.CreateAsync(dto);
            if (!success)
            {
                return error switch
                {
                    "BookNotFound" => NotFound(new { error = "Book not found" }),
                    "UserNotFound" => NotFound(new { error = "User not found" }),
                    "BookAvailableLoanInstead" => Conflict(new { error = "Book is available — loan instead" }),
                    "AlreadyReserved" => Conflict(new { error = "You already have an active/ready reservation for this book" }),
                    _ => BadRequest(new { error })
                };
            }

            return CreatedAtAction(nameof(Get), new { id = reservation!.Id }, reservation);
        }

        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _svc.CancelAsync(id);
            if (!ok) return BadRequest(new { error = "Reservation not found or cannot be cancelled" });
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            var (success, error) = await _svc.UpdateAsync(id, dto);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    _ => BadRequest(new { error })
                };
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        // NEW: get queue overview for a book
        // GET: api/Reservation/book/{bookId}/queue
        [HttpGet("book/{bookId:int}/queue")]
        public async Task<IActionResult> GetQueue(int bookId)
        {
            var info = await _svc.GetQueueForBookAsync(bookId);
            return Ok(info);
        }

        // NEW: manual expiry scan
        // POST: api/Reservation/run-expiry-scan
        [HttpPost("run-expiry-scan")]
        public async Task<IActionResult> RunExpiryScan()
        {
            var n = await _svc.ExpireReadyReservationsAsync();
            return Ok(new { expired = n });
        }
    }
}
