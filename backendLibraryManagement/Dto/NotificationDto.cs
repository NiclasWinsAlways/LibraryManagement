namespace backendLibraryManagement.Dto
{
    // DTO returned when fetching user notifications.
    public class NotificationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = ""; // Notification content shown to the user.
        public DateTime CreatedAt { get; set; } // Timestamp for when notification was created.
        public bool IsRead { get; set; } // True if the user has read the notification.
    }
}
