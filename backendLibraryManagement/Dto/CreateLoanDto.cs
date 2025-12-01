namespace backendLibraryManagement.Dto
{
    // DTO used when a new loan is created.
    // Links a user and a book together with a due date.
    public class CreateLoanDto
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime EndDate { get; set; } // Date the loan must be returned.
    }
}
