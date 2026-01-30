namespace backendLibraryManagement.Model
{
    // Many-to-many: User <-> Book (wishlist/favorites)
    public class Favorite
    {
        public int Id { get; set; }                 // simple PK (easiest)
        public int UserId { get; set; }
        public User? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
