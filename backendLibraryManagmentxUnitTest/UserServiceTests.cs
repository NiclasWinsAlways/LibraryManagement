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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backendLibraryManagmentxUnitTest
{
    public class UserServiceTests
    {
        private LibraryContext GetDb()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new LibraryContext(options);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_User_With_Hashed_Password()
        {
            var db = GetDb();
            var svc = new UserService(db);

            var dto = new CreateUserDto
            {
                Name = "Test",
                Email = "test@test.dk",
                Password = "secret",
                Role = "Admin"
            };
            var created = await svc.CreateAsync(dto);
            var user = await db.Users.FirstAsync();
            Assert.Equal("Test", user.Name);
            Assert.NotEqual("secret", user.PasswordHash);
        }

        [Fact]
        public async Task UpdateAsync_Should_Fail_When_Email_Exists()
        {
            var db = GetDb();
            db.Users.Add(new User { Id = 1, Email = "a@test.dk" });
            db.Users.Add(new User { Id = 2, Email = "b@test.dk" });
            await db.SaveChangesAsync();
            var svc = new UserService(db);
            var dto = new UpdateUserDto { Email = "b@test.dk" };
            var (success, error) = await svc.UpdateAsync(1, dto);
            Assert.False(success);
            Assert.Equal("EmailExists", error);
        }
        [Fact]
        public async Task VerifyPasswordAsync_Should_Return_True_When_Correct()
        {
            var db = GetDb();
            var svc = new UserService(db);
            var hash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes("pw"))
            );
            db.Users.Add(new User { Email = "x@x.dk", PasswordHash = hash });
            await db.SaveChangesAsync();
            var ok = await svc.VerifyPasswordAsync("x@x.dk", "pw");
            Assert.True(ok);
        }
    }
}
