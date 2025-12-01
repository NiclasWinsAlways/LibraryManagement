namespace backendLibraryManagement.Dto
{
    // DTO used when updating book details.
    public class UpdateBookDto
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int TotalCopies { get; set; } // Total number of copies owned by the library.
        public int CopiesAvailable { get; set; } // Current number of available copies.
        public bool IsAvailable { get; set; } // Whether the book can currently be loaned.
    }
}
