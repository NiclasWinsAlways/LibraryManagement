namespace backendLibraryManagement.Dto
{
    public class TopBookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = "";
        public string? Author { get; set; }
        public int LoanCount { get; set; }
    }
}
