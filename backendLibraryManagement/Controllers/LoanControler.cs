using Microsoft.AspNetCore.Mvc;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanControler : ControllerBase
    {
        private readonly LoanService _svc;
        public LoanControler(LoanService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var loan = await _svc.GetByIdAsync(id);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLoanDto dto)
        {
            var (success, error, loan) = await _svc.CreateAsync(dto);
            if (!success) return BadRequest(new { error });
            return CreatedAtAction(nameof(Get), new { id = loan!.Id }, loan);
        }

        [HttpPost("{id:int}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var ok = await _svc.ReturnLoanAsync(id);
            if (!ok) return BadRequest(new { error = "Loan not found or already returned" });
            return NoContent();
        }
    }
}