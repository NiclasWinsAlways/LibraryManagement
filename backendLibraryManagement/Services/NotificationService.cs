using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class NotificationService
    {
        private readonly LibraryContext _db;
        public NotificationService(LibraryContext db)=> _db = db;
        public async Task CreateAsync(int userId, string message)
        {
            var n = new Notification
            {
                UserId = userId,
                Message = message
            };
            _db.Notifications.Add(n);
            await  _db.SaveChangesAsync();
        }
        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.IsRead
                })
                .ToListAsync();
        }
        public async Task MarkAsReadAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if(n!=null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
        public async Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateNotificationDto dto)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null) return (false, "NotFound");

            if (dto.IsRead.HasValue)
                n.IsRead = dto.IsRead.Value;

            if (!string.IsNullOrWhiteSpace(dto.Message))
                n.Message = dto.Message.Trim();

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null) return false;

            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();
            return true;
        }

    }
}
