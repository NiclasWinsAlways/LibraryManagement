namespace backendLibraryManagement.Model
{
    // Represents a reservation placed by a user for a book.
    // Reservations ensure queueing and fairness when copies are unavailable.
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; } // Foreign key to the reserved book.
        public Book? Book { get; set; } // Navigation property to the reserved book.
        public int UserId { get; set; } // Foreign key to the user making the reservation.
        public User? User { get; set; } // Navigation property to the reserving user.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp for when the reservation was created.
        public string Status { get; set; } = "Active"; // Status of the reservation:"Active" | "Cancelled" | "Fulfilled"
        public DateTime? ExpiresAt { get; set; } // Optional expiration date for the reservation.
    }
}
