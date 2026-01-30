using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace backendLibraryManagmentxUnitTest
{
    public class ReservationServiceTests
    {
        private LibraryContext GetDb()
        {
            return new LibraryContext(
                new DbContextOptionsBuilder<LibraryContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            );
        }

        // ✅ Helper to create fake NotificationService (no real emails)
        private INotificationService GetNotificationService()
        {
            var notifMock = new Mock<INotificationService>();
            notifMock
                .Setup(n => n.CreateAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            return notifMock.Object;
        }

        [Fact]
        public async Task CreateAsync_Should_Fail_When_Book_Has_Available_Copies()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, CopiesAvailable = 1});
            db.Users.Add(new User { Id = 1 });
            await db.SaveChangesAsync();
            var svc = new ReservationService(db, GetNotificationService());
            var dto = new CreateReservationDto { BookId = 1, UserId = 2 };

            var (success, error, _) = await svc.CreateAsync(dto);

            Assert.False(success);
            Assert.Equal("Book currently available — please loan instead", error);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_When_No_Copies()
        {
            var db = GetDb();
            db.Books.Add(new Book { Id = 1, Title = "Bog", CopiesAvailable = 0});
            db.Users.Add(new User { Id = 2, Name = "U" });
            await db.SaveChangesAsync();

            var svc = new ReservationService(db, GetNotificationService());
            var dto = new CreateReservationDto { BookId = 1, UserId = 2 };

            var (ok, _, res) = await svc.CreateAsync(dto);

            Assert.True(ok);
            Assert.Equal(1, res.BookId);
            Assert.Equal(2, res.UserId);
        }

        [Fact]
        public async Task CancelAsync_Should_Set_Status_To_Cancelled()
        {
            var db = GetDb();
            var user = new User { Id = 1, Name = "Test" };
            var book = new Book { Id = 1, Title = "Book" };
            db.Users.Add(user);
            db.Books.Add(book);
            await db.SaveChangesAsync();

            var res = new Reservation { Status = "Active", UserId = 1, BookId = 1 };
            db.Reservations.Add(res);
            await db.SaveChangesAsync();
            await db.Entry(res).ReloadAsync();

            var svc = new ReservationService(db, GetNotificationService());

            var ok = await svc.CancelAsync(res.Id);

            Assert.True(ok);
            var updated = await db.Reservations.FindAsync(res.Id);
            Assert.Equal("Cancelled", updated!.Status);
        }
    }
}
