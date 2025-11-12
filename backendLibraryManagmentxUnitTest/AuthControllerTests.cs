using Xunit;
using Moq;
using backendLibraryManagement.Controllers;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;
using Microsoft.AspNetCore.Mvc;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
    {
        var mock = new Mock<AuthService>(null!, null!);
        mock.Setup(s => s.AuthenticateAsync("a@a.dk", "123"))
            .ReturnsAsync((false, null, "Invalid credentials"));

        var ctrl = new AuthController(mock.Object);

        var result = await ctrl.Login(new LoginDto { Email = "a@a.dk", Password = "123" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenSuccess()
    {
        var mock = new Mock<AuthService>(null!, null!);
        mock.Setup(s => s.AuthenticateAsync("a@a.dk", "123"))
            .ReturnsAsync((true, "TOKEN123", null));

        var ctrl = new AuthController(mock.Object);

        var result = await ctrl.Login(new LoginDto { Email = "a@a.dk", Password = "123" }) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal("TOKEN123", result.Value.GetType().GetProperty("token")!.GetValue(result.Value));
    }
}
