using Microsoft.EntityFrameworkCore;
using backendLibraryManagement.Model;

namespace backendLibraryManagement.Data
{
    public class LibraryContext : DbContext
    {
        // Constructor modtager DbContext options fra DI (eks. connection string)
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }
        // DbSet repræsenterer tabellerne i databasen
        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LOAN RELATIONER
            // En Loan har én Book, og én Book kan have mange Loans (historik)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany() // Book behøver ikke navigation tilbage (One-to-Many uden collection)
                .HasForeignKey(l => l.BookId)  // FK i Loan-tabellen
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade: Hvis en bog slettes, slettes dens loans (logisk i bibliotekssystem)

            // En Loan har én User, og en User kan have mange Loans
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans) // Navigation: User.Loans
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Hvis en user slettes, slettes deres lån

            // RESERVATION RELATIONER
            // En Reservation er knyttet til én Book, som kan have mange reservationer
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany() // Book behøver ikke en liste af reservationer
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);
            // Hvis en Book slettes, slettes dens reservationer

            // En User kan have mange Reservationer (hvis ønsket)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany() // Kan udvides til User.Reservations
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Hvis en User slettes, fjernes deres reservationer

            // AFSLUT MODEL KONFIGURATION
            base.OnModelCreating(modelBuilder);

            // seed database 
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", ISBN = "9780007458424", CopiesAvailable = 3, IsAvailable = true },
                new Book { Id = 2, Title = "1984", Author = "George Orwell", Genre = "Dystopian", ISBN = "9780451524935", CopiesAvailable = 5, IsAvailable = true },
                new Book { Id = 3, Title = "Clean Code", Author = "Robert C. Martin", Genre = "Programming", ISBN = "9780132350884", CopiesAvailable = 2, IsAvailable = true }
            );
        }
    }
}
