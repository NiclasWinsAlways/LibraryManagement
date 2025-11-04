namespace backendLibraryManagement.Model
{
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Active / Cancelled / Fulfilled
        public string Status { get; set; } = "Active";
    }
}
