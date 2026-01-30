namespace backendLibraryManagement.Dto
{
    // DTO used when updating user information.
    // All fields are optional to allow partial updates.
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get;set; } // If null/empty: password is not changed.
        public string? Role { get;set; } // Only applied if caller is authorized (e.g., admin).

        // Profile fields
        public string? PhoneNumber { get; set; }
        public bool? SmsOptIn { get; set; }
        public bool? EmailOptIn { get; set; }
    }
}
