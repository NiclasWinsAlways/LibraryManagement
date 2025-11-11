using backendLibraryManagement.Dto;
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
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNotificationDto dto)
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

    }
}
