using Microsoft.EntityFrameworkCore;
using backendLibraryManagement.Model;

namespace backendLibraryManagement.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // En Loan har én Book (FK BookId)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany() // En bog kan have mange lån over tid
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict); // Undgå cascade delete

            // En Loan har én User, og en User kan have mange Loans
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // book kan have mange reservationer over tid
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            //user kan have mange reservationer hvis du ønske
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
