namespace backendLibraryManagement.Dto
{
    public class UpdateBookDto
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int CopiesAvailable { get; set; }
        public bool IsAvailable { get; set; }
    }
}
