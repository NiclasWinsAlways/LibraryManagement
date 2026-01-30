using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _svc;
        public LoanController(ILoanService svc) => _svc = svc;

        [HttpGet("getloans")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var loan = await _svc.GetByIdAsync(id);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLoanDto dto)
        {
            var (success, error, loan) = await _svc.CreateAsync(dto);
            if (!success)
            {
                return error switch
                {
                    "BookNotFound" => NotFound(new { error = "Book not found" }),
                    "UserNotFound" => NotFound(new { error = "User not found" }),
                    "BookNotAvailable" => Conflict(new { error = "Book not available; consider reserving" }),
                    "ReservedForAnotherUser" => Conflict(new { error = "Book is reserved for another user (Ready reservation)" }),
                    "QueueExists" => Conflict(new { error = "There is a reservation queue for this book; you are not first in line" }),
                    _ => BadRequest(new { error })
                };
            }
            return CreatedAtAction(nameof(Get), new { id = loan!.Id }, loan);
        }

        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var ok = await _svc.ReturnLoanAsync(id);
            if (!ok) return BadRequest(new { error = "Loan not found or already returned" });
            return NoContent();
        }

        // NEW: extend loan
        // POST: api/Loan/{id}/extend?days=7
        [HttpPost("{id:int}/extend")]
        public async Task<IActionResult> Extend(int id, [FromQuery] int days = 7)
        {
            var (success, error, loan) = await _svc.ExtendLoanAsync(id, days);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    "InvalidDays" => BadRequest(new { error = "Invalid extension days" }),
                    "LoanNotActive" => BadRequest(new { error = "Loan is not active" }),
                    "LoanOverdue" => BadRequest(new { error = "Loan is overdue and cannot be extended" }),
                    "MaxExtensionsReached" => BadRequest(new { error = "Max extensions reached" }),
                    "ReservedByOthers" => Conflict(new { error = "Book is reserved by other users; extension not allowed" }),
                    _ => BadRequest(new { error })
                };
            }

            return Ok(loan);
        }
    }
}
