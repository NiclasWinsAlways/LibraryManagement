namespace backendLibraryManagement.Dto
{
    public class BookRatingSummaryDto
    {
        public int BookId { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
