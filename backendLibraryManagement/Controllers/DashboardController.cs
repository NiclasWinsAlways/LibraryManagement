using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _svc;
        public DashboardController(IDashboardService svc) => _svc = svc;

        // GET: api/Dashboard/summary?dueWithinDays=2
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] int dueWithinDays = 2)
        {
            var data = await _svc.GetSummaryAsync(dueWithinDays);
            return Ok(data);
        }

        // GET: api/Dashboard/top-books?days=30&take=10
        [HttpGet("top-books")]
        public async Task<IActionResult> TopBooks([FromQuery] int days = 30, [FromQuery] int take = 10)
        {
            var data = await _svc.GetTopBooksAsync(days, take);
            return Ok(data);
        }

        // GET: api/Dashboard/loans-trend?days=30
        [HttpGet("loans-trend")]
        public async Task<IActionResult> LoansTrend([FromQuery] int days = 30)
        {
            var data = await _svc.GetLoansTrendAsync(days);
            return Ok(data);
        }
    }
}
