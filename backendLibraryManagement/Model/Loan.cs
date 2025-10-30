namespace backendLibraryManagement.Model
{
    public class Loan
    {
        public int Id { get; set; }

        // FK til Book
        public int BookId { get; set; }
        public Book? Book { get; set; }

        // FK til User
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Aktiv"; // Aktiv / Afleveret / Forfalden
    }
}
