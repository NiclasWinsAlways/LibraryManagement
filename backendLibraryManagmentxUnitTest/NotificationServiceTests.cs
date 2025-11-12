using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backendLibraryManagmentxUnitTest
{
    public class NotificationServiceTests
    {
        private LibraryContext GetDb()
        {
            return new LibraryContext(
                new DbContextOptionsBuilder<LibraryContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            );
        }
        [Fact]
        public async Task CreateAsync_Should_Add_Notification()
        {
            var db = GetDb();
            var svc = new NotificationService(db);
            await svc.CreateAsync(1, "Hello");
            Assert.Single(db.Notifications);
            Assert.Equal("Hello", db.Notifications.First().Message);
        }
        [Fact]
        public async Task UpdateAsync_Should_Update_Message_And_IsRead()
        {
            var db = GetDb();
            db.Notifications.Add(new Notification { Id = 10, Message = "old", IsRead = false });
            await db.SaveChangesAsync();
            var svc = new NotificationService(db);
            var dto = new UpdateNotificationDto { Message = "new", IsRead = true };
            var (ok, _) = await svc.UpdateAsync(10, dto);
            Assert.True(ok);
            var n = db.Notifications.Find(10)!;
            Assert.Equal("new", n.Message);
            Assert.True(n.IsRead);
        }
        [Fact]
        public async Task DeleteAsync_Should_Remove_Notification()
        {
            var db = GetDb();
            db.Notifications.Add(new Notification { Id = 55 });
            await db.SaveChangesAsync();
            var svc = new NotificationService(db);
            var ok = await svc.DeleteAsync(55);
            Assert.True(ok);
            Assert.Empty(db.Notifications);
        }
    }
}
