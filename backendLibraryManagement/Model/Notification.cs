namespace backendLibraryManagement.Model
{
    // Represents a user notification (e.g., upcoming due date, reservation ready).
    // Notifications can be read or unread and belong to a specific user.
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; } // Foreign key to the user receiving the notification.
        public User? User { get; set; } // Navigation property to the notification's user.
        public string Message { get; set; } = ""; // The text message shown to the user.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for when the notification was generated.
        public bool IsRead { get; set; } = false; // Indicates whether the user has marked the notification as read.
    }
}
