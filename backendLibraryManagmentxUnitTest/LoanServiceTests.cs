using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace backendLibraryManagmentxUnitTest
{
    public class LoanServiceTests
    {
        private LibraryContext GetDb()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new LibraryContext(options);
        }

        // helper to create LoanService with NotificationService
        private LoanService CreateLoanService(LibraryContext db)
        {
            var notif = new NotificationService(db);
            return new LoanService(db, notif);
        }

        [Fact]
        public async Task CreateLoan_ShouldFail_WhenNoCopiesAvailable()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, Title = "Bog", CopiesAvailable = 0, IsAvailable = false });
            db.Users.Add(new User { Id = 2, Name = "User" });
            await db.SaveChangesAsync();
            var svc = CreateLoanService(db);
            var dto = new CreateLoanDto { BookId = 1, UserId = 2, EndDate = DateTime.UtcNow.AddDays(5) };
            var (success, error, loan) = await svc.CreateAsync(dto);
            Assert.False(success);
            Assert.Equal("Book not available; consider reserving it", error);
        }

        [Fact]
        public async Task CreateLoan_ShouldFail_WhenUserNotFound()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, CopiesAvailable = 1, IsAvailable = true });
            await db.SaveChangesAsync();
            var svc = CreateLoanService(db);
            var dto = new CreateLoanDto { BookId = 1, UserId = 999, EndDate = DateTime.UtcNow.AddDays(1) };
            var (success, error, _) = await svc.CreateAsync(dto);
            Assert.False(success);
            Assert.Equal("User not found", error);
        }

        [Fact]
        public async Task CreateLoan_Should_Decrease_Copies()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, Title = "Bog", CopiesAvailable = 2, IsAvailable = true });
            db.Users.Add(new User { Id = 2 });
            await db.SaveChangesAsync();
            var svc = CreateLoanService(db);
            var dto = new CreateLoanDto { BookId = 1, UserId = 2, EndDate = DateTime.UtcNow };
            var (ok, _, _) = await svc.CreateAsync(dto);
            Assert.True(ok);
            var book = db.Books.Find(1)!;
            Assert.Equal(1, book.CopiesAvailable);
        }

        [Fact]
        public async Task ReturnLoan_Should_Set_Status_And_Increase_Copies()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, CopiesAvailable = 0, IsAvailable = false });
            db.Users.Add(new User { Id = 10 });
            db.Loans.Add(new Loan { Id = 5, BookId = 1, UserId = 10, Status = "Aktiv" });
            await db.SaveChangesAsync();
            var svc = CreateLoanService(db);
            var ok = await svc.ReturnLoanAsync(5);
            var loan = db.Loans.Find(5)!;
            var book = db.Books.Find(1)!;
            Assert.True(ok);
            Assert.Equal("Afleveret", loan.Status);
            Assert.Equal(1, book.CopiesAvailable);
            Assert.True(book.IsAvailable);
        }
    }
}