namespace backendLibraryManagement.Dto
{
    // DTO used to return user data to the client.
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = ""; // User role (Admin / Låner / etc.)
    }
}
