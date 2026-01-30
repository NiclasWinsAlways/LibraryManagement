namespace backendLibraryManagement.Model
{
    // Represents a loan made by a user.
    // A loan links a specific user and book for a period of time.
    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; } // Foreign key to the borrowed book.
        public Book? Book { get; set; } // Navigation property to the book being loaned.
        public int UserId { get; set; } // Foreign key to the user borrowing the book.
        public User? User { get; set; } // Navigation property to the user who borrowed the book.
        public DateTime StartDate { get; set; } = DateTime.UtcNow; // The date the loan was created.
        public DateTime EndDate { get; set; } // The mandatory return date.
        public DateTime? ReturnedAt { get; set; } // The actual return date, null if not yet returned.
        public int ExtendedCount { get; set; } = 0;// Number of times the loan has been extended.
        public string Status { get; set; } = "Aktiv"; // Current status of the loan:"Aktiv" (active), "Afleveret" (returned), "Forfalden" (overdue)

    }
}
