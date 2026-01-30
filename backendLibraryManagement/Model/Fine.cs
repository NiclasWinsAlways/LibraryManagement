namespace backendLibraryManagement.Model
{
    // Fine for an overdue loan
    public class Fine
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int LoanId { get; set; }
        public Loan? Loan { get; set; }

        public decimal Amount { get; set; }          // DKK
        public string Reason { get; set; } = "Overdue loan";

        // "Unpaid" | "Paid" | "Waived"
        public string Status { get; set; } = "Unpaid";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
    }
}
