namespace backendLibraryManagement.Dto
{
    public class CreateUserDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Låner";
    }
}
