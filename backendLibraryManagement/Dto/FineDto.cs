namespace backendLibraryManagement.Dto
{
    public class FineDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int LoanId { get; set; }

        public decimal Amount { get; set; }
        public string Reason { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
