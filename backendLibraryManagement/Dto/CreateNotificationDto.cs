namespace backendLibraryManagement.Dto
{
    // DTO used when creating a new notification for a user.
    public class CreateNotificationDto
    {
        public int UserId { get; set; }
        public string Message { get; set; } = ""; // The message shown to the user.
    }
}
