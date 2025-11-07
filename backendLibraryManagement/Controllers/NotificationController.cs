using backendLibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController:ControllerBase
    {
        private readonly NotificationService _svc;
        public NotificationController(NotificationService svc)
        {
            _svc = svc;
        }
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            return Ok(await _svc.GetUserNotificationsAsync(userId));
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _svc.MarkAsReadAsync(id);
            return NoContent();
        }
    }
}
