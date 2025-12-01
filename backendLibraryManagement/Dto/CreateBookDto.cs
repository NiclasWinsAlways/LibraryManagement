namespace backendLibraryManagement.Dto
{
    // DTO used when creating a new book record.
    // Only includes fields required from client input.
    public class CreateBookDto
    {
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Author { get; set; } = "";
        public string ISBN { get; set; } = "";
        public int TotalCopies { get; set; } // Total number of copies owned by the library.
    }
}
