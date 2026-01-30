namespace backendLibraryManagement.Dto
{
    // DTO used when returning book data to the client.
    // Represents a read-only projection of book information.
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Author { get; set; } = "";
        public string ISBN { get; set; } = "";

        public int TotalCopies { get; set; }
        public int CopiesAvailable { get; set; } // Number of copies available for borrowing.
        public bool IsAvailable { get; set; } // Indicates whether the book is currently available for loan.
    }
}
