namespace backendLibraryManagement.Dto
{
    public class CreateReviewDto
    {
        public int UserId { get; set; }
        public int Rating { get; set; }   // 1..5
        public string? Comment { get; set; }
    }
}
