using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FineController : ControllerBase
    {
        private readonly IFineService _svc;
        public FineController(IFineService svc) => _svc = svc;

        // GET: api/Fine/user/5
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            var fines = await _svc.GetFinesForUserAsync(userId);
            return Ok(fines);
        }

        // POST: api/Fine/run-overdue-scan
        [HttpPost("run-overdue-scan")]
        public async Task<IActionResult> RunOverdueScan()
        {
            var changed = await _svc.RunOverdueFineScanAsync();
            return Ok(new { changed });
        }

        // POST: api/Fine/{fineId}/pay
        [HttpPost("{fineId:int}/pay")]
        public async Task<IActionResult> Pay(int fineId, [FromBody] PayFineDto dto)
        {
            var (success, error, receipt) = await _svc.PayFineAsync(fineId, dto.UserId);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    "Forbidden" => Forbid(),
                    "AlreadyPaid" => Conflict(new { error = "Fine is already paid" }),
                    "NotPayable" => BadRequest(new { error = "Fine is not payable" }),
                    _ => BadRequest(new { error })
                };
            }

            return Ok(receipt);
        }

        // GET: api/Fine/receipt/10
        [HttpGet("receipt/{receiptId:int}")]
        public async Task<IActionResult> GetReceipt(int receiptId)
        {
            var r = await _svc.GetReceiptAsync(receiptId);
            if (r == null) return NotFound();
            return Ok(r);
        }

        // GET: api/Fine/user/5/receipts
        [HttpGet("user/{userId:int}/receipts")]
        public async Task<IActionResult> GetReceiptsForUser(int userId)
        {
            var r = await _svc.GetReceiptsForUserAsync(userId);
            return Ok(r);
        }
    }
}
