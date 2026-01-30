namespace backendLibraryManagement.Dto
{
    public class ReviewDto
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public string? BookTitle { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
