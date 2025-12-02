using backendLibraryManagement.Dto;
using backendLibraryManagement.Services;
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

        // GET: api/Reservation/getReservations
        // Returns all reservations.
        [HttpGet("getReservations")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        // GET: api/Reservation/{id}
        // Returns a single reservation.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var res = await _svc.GetByIdAsync(id);
            if (res == null) return NotFound();
            return Ok(res);
        }

        // POST: api/Reservation/create
        // Creates a reservation for a book.
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            var (success, error, reservation) = await _svc.CreateAsync(dto);
            if (!success) return BadRequest(new { error });
            return CreatedAtAction(nameof(Get), new { id = reservation!.Id }, reservation);
        }

        // POST: api/Reservation/{id}/cancel
        // Cancels a reservation if possible.
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _svc.CancelAsync(id);
            if (!ok) return BadRequest(new { error = "Reservation not found or cannot be cancelled" });
            return NoContent();
        }

        // PUT: api/Reservation/{id}
        // Updates a reservation.
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

        // DELETE: api/Reservation/{id}
        // Deletes a reservation entirely.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

    }
}