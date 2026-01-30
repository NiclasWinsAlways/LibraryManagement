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
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // LOAN relations
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany()
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // RESERVATION relations
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index helps queue lookup
            modelBuilder.Entity<Reservation>()
                .HasIndex(r => new { r.BookId, r.Status, r.CreatedAt });

            modelBuilder.Entity<Favorite>()
                .HasOne(f=>f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Book)
                .WithMany()
                .HasForeignKey(f => f.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.BookId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One review per (User, Book)
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.BookId, r.UserId })
                .IsUnique();

            // Optional: index by book rating lookups
            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.BookId, r.CreatedAt });

            // FINE relations
            modelBuilder.Entity<Fine>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Fine>()
                .HasOne(f => f.Loan)
                .WithMany()
                .HasForeignKey(f => f.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // One unpaid fine per loan (enforced by logic; index helps lookup)
            modelBuilder.Entity<Fine>()
                .HasIndex(f => new { f.LoanId, f.Status });

            // RECEIPT relations
            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Receipt>()
                .HasOne(r => r.Fine)
                .WithMany()
                .HasForeignKey(r => r.FineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Receipt>()
                .HasIndex(r => r.ReceiptNumber)
                .IsUnique();

            // DECIMAL precision for money values (SQL Server)
            modelBuilder.Entity<Fine>()
                .Property(f => f.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Receipt>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);

            // Seed books (UPDATED: TotalCopies + CopiesAvailable)
            modelBuilder.Entity<Book>().HasData(
                new Book { Id = 1, Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", ISBN = "9780007458424", TotalCopies = 3, CopiesAvailable = 3 },
                new Book { Id = 2, Title = "1984", Author = "George Orwell", Genre = "Dystopian", ISBN = "9780451524935", TotalCopies = 5, CopiesAvailable = 5 },
                new Book { Id = 3, Title = "Clean Code", Author = "Robert C. Martin", Genre = "Programming", ISBN = "9780132350884", TotalCopies = 2, CopiesAvailable = 2 }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
