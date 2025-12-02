using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagement.Services
{
    // Handles app-level notifications for users.
    // Notifications are stored in the database and optionally emailed to the user.
    public class NotificationService: INotificationService
    {
        private readonly LibraryContext _db;
        private readonly IEmailService _email;
        public NotificationService(LibraryContext db,IEmailService email)=> (_db,_email) = (db,email);

        // Creates a new notification and optionally sends an email to the user.
        public async Task CreateAsync(int userId, string message)
        {
            var n = new Notification
            {
                UserId = userId,
                Message = message
            };
            _db.Notifications.Add(n);
            await  _db.SaveChangesAsync();

            // Send email version of notification, if user has an email. 
            var user = await _db.Users.FindAsync(userId);
            if(user!= null && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _email.SendEmailAsync(
                    to: user.Email,
                    subject: "Library Notification",
                    body: message
                );
            }
        }

        // Returns all notifications for a user, newest first.
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

        // Marks a notification as read.
        public async Task MarkAsReadAsync(int id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null) return;
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }

        // Scans for users with due loans within the next two days.
        // Sends a reminder notification to each affected user.
        public async Task NotifyUpcomingDueDatesAsync()
        {
            var now = DateTime.UtcNow;

            var from = now;           // include today
            var to = now.AddDays(2);  // include next 2 days

            var loans = await _db.Loans
                .Include(l => l.Book)
                .Where(l => l.Status == "Aktiv" &&
                            l.EndDate.Date >= from &&
                            l.EndDate.Date <= to)
                .ToListAsync();

            foreach (var loan in loans)
            {
                var title = loan.Book?.Title ?? "a book";
                var message = $"Reminder: your loan of '{title}' is due on {loan.EndDate:yyyy-MM-dd}. Please return or extend it.";
                await CreateAsync(loan.UserId, message);
            }
        }

        // Updates an existing notification.
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

        // Deletes a notification.
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
