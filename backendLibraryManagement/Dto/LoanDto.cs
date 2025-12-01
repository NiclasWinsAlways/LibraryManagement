namespace backendLibraryManagement.Dto
{
    // DTO representing loan information returned to the client.
    public class LoanDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime StartDate { get; set; } // The date the book was borrowed.
        public DateTime EndDate { get; set; } // The date the book must be returned.
        public string Status { get; set; } = ""; // Computed loan status (Active, Returned, Overdue)
    }
}
