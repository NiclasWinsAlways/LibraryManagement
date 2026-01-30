using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly LibraryContext _db;
        private readonly IEmailService _email;
        private readonly ISmsService _sms;

        public NotificationService(LibraryContext db, IEmailService email, ISmsService sms)
            => (_db, _email, _sms) = (db, email, sms);

        public async Task CreateAsync(int userId, string message)
        {
            var n = new Notification
            {
                UserId = userId,
                Message = message
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            // Fetch user for delivery
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            // Email (existing behavior)
            if (!string.IsNullOrWhiteSpace(user.Email) && user.EmailOptIn)
            {
                try
                {
                    await _email.SendEmailAsync(
                        to: user.Email,
                        subject: "Library Notification",
                        body: message
                    );
                }
                catch
                {
                    // Keep notification stored even if sending fails
                }
            }

            // SMS (NEW)
            if (user.SmsOptIn && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                try
                {
                    await _sms.SendSmsAsync(user.PhoneNumber.Trim(), message);
                }
                catch
                {
                    // ignore send error for now
                }
            }
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
            if (n == null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task NotifyUpcomingDueDatesAsync()
        {
            var fromDate = DateTime.UtcNow.Date;
            var toDate = fromDate.AddDays(2);

            var loans = await _db.Loans
                .Include(l => l.Book)
                .Where(l => l.Status == "Aktiv" &&
                            l.EndDate.Date >= fromDate &&
                            l.EndDate.Date <= toDate)
                .ToListAsync();

            foreach (var loan in loans)
            {
                var title = loan.Book?.Title ?? "a book";
                var msg = $"Reminder: your loan of '{title}' is due on {loan.EndDate:yyyy-MM-dd}. Please return or extend it.";
                await CreateAsync(loan.UserId, msg);
            }
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
