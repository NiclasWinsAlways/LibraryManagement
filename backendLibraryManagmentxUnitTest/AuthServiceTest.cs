using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backendLibraryManagement.Data;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;

namespace backendLibraryManagmentxUnitTest
{
    public class AuthServiceTest
    {
        private static LibraryContext GetDb()
        {
            var options = new DbContextOptionsBuilder<LibraryContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new LibraryContext(options);
        }

        private static IConfiguration GetConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "Jwt:Key", "12345678901234567890123456789012" }, // 32 chars
                    { "Jwt:Issuer", "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" },
                    { "Jwt:ExpireMinutes", "10" }
                })
                .Build();

        private static (AuthService auth, UserService users, LibraryContext db) BuildSystem()
        {
            var db = GetDb();
            var users = new UserService(db);
            var auth = new AuthService(GetConfig(), users);
            return (auth, users, db);
        }

        [Fact]
        public async Task Authenticate_ShouldFail_WhenUserNotFound()
        {
            var (auth, _, _) = BuildSystem();

            var (success, token, err) = await auth.AuthenticateAsync("missing@x.dk", "123");

            Assert.False(success);
            Assert.Null(token);
            Assert.Equal("Invalid credentials", err);
        }

        [Fact]
        public async Task Authenticate_ShouldFail_WhenPasswordWrong()
        {
            var (auth, users, _) = BuildSystem();

            // Create a real user with password "correct"
            await users.CreateAsync(new CreateUserDto
            {
                Name = "U",
                Email = "u@x.dk",
                Password = "correct",
                Role = "Member"
            });

            var (success, token, err) = await auth.AuthenticateAsync("u@x.dk", "wrong");

            Assert.False(success);
            Assert.Null(token);
            Assert.Equal("Invalid credentials", err);
        }

        [Fact]
        public async Task Authenticate_ShouldReturnToken_WhenCorrect()
        {
            var (auth, users, _) = BuildSystem();

            await users.CreateAsync(new CreateUserDto
            {
                Name = "U",
                Email = "u@x.dk",
                Password = "secret123",
                Role = "Admin" // any allowed role is fine
            });

            var (success, token, err) = await auth.AuthenticateAsync("u@x.dk", "secret123");

            Assert.True(success);
            Assert.NotNull(token);
            Assert.Null(err);
        }
    }
}
