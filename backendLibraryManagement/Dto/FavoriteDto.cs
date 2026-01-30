namespace backendLibraryManagement.Dto
{
    public class FavoriteDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int BookId { get; set; }

        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
