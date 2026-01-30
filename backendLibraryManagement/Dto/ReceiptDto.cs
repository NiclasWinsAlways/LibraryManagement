namespace backendLibraryManagement.Dto
{
    public class ReceiptDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FineId { get; set; }

        public string ReceiptNumber { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime IssuedAt { get; set; }
    }
}
