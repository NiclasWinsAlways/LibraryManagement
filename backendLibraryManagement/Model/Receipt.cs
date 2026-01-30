namespace backendLibraryManagement.Model
{
    // Receipt for a fine payment
    public class Receipt
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int FineId { get; set; }
        public Fine? Fine { get; set; }

        public string ReceiptNumber { get; set; } = "";   // unique
        public decimal Amount { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
