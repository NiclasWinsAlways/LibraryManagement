using backendLibraryManagement.Dto;

namespace backendLibraryManagement.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(int userId, string message);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int id);
        Task NotifyUpcomingDueDatesAsync();
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateNotificationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
