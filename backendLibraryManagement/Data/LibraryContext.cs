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
        public DbSet<Notifications Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // En Loan har én Book (FK BookId)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Book)
                .WithMany() // En bog kan have mange lån over tid
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Cascade); // Undgå cascade delete

            // En Loan har én User, og en User kan have mange Loans
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // book kan have mange reservationer over tid
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Book)
                .WithMany()
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            //user kan have mange reservationer hvis du ønske
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
