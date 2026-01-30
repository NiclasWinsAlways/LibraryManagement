namespace backendLibraryManagement.Dto
{
    // For /me endpoint: user can only update own profile (no role)
    public class UpdateMyProfileDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

        public string? PhoneNumber { get; set; }
        public bool? SmsOptIn { get; set; }
        public bool? EmailOptIn { get; set; }
    }
}
