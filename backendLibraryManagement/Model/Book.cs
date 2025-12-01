namespace backendLibraryManagement.Model
{
    // Represents a book in the library's collection.
    // This entity tracks availability and basic bibliographic information.
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = ""; // Title of the book.
        public string Genre { get; set; } = ""; // Fiction / Non-fiction / Thriller / History etc.
        public string Author { get; set; } = ""; // Name of the book's author.
        public string ISBN { get; set; } = ""; // International Standard Book Number used for identification. vores version
        public int CopiesAvailable { get; set; }  // Number of copies currently available for loan.
        public bool IsAvailable { get; set; } // Indicates whether the book can be borrowed (CopiesAvailable > 0).
    }
}
