namespace backendLibraryManagement.Dto
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        // Optional: when null/empty no password change
        public string? Password { get;set; }
        // Optional: only applied when caller is authorized to change
        public string? Role { get;set; }
    }
}
