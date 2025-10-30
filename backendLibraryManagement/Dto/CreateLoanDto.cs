namespace backendLibraryManagement.Dto
{
    public class CreateLoanDto
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime EndDate { get; set; }
    }
}
