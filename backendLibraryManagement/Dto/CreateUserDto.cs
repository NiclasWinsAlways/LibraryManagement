namespace backendLibraryManagement.Dto
{
    // DTO used when creating a new user account.
    public class CreateUserDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Låner"; // Default role is "Låner" (borrower) unless specified otherwise.
    }
}
