using Microsoft.AspNetCore.Mvc;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly LoanService _svc;
        public LoanController(LoanService svc) => _svc = svc;

        // GET: api/Loan
        [HttpGet("getloans")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        // GET: api/Loan/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var loan = await _svc.GetByIdAsync(id);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        // POST: api/Loan
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLoanDto dto)
        {
            var (success, error, loan) = await _svc.CreateAsync(dto);
            if (!success) return BadRequest(new { error });
            return CreatedAtAction(nameof(Get), new { id = loan!.Id }, loan);
        }

        // POST: api/Loan/{id}/return
        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var ok = await _svc.ReturnLoanAsync(id);
            if (!ok) return BadRequest(new { error = "Loan not found or already returned" });
            return NoContent();
        }
    }
}