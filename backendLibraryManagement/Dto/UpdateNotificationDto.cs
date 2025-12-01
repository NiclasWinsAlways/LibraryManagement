namespace backendLibraryManagement.Dto
{
    // DTO used when updating a notification.
    // All fields are optional.
    public class UpdateNotificationDto
    {
        public bool? IsRead { get; set; } // Null means no change.
        public string? Message { get; set; } // Used when editing the message content.
    }
}
