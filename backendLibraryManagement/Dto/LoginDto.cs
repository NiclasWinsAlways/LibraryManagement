namespace backendLibraryManagement.Dto
{
    // DTO used for login requests.
    // Contains user credentials for authentication.
    public class LoginDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
