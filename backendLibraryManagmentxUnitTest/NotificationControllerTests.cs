using Xunit;
using Moq;
using backendLibraryManagement.Controllers;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NotificationControllerTests
{
    [Fact]
    public async Task GetForUser_ShouldReturnNotifications()
    {
        var mock = new Mock<NotificationService>(null!);

        mock.Setup(s => s.GetUserNotificationsAsync(1))
            .ReturnsAsync(new List<NotificationDto>
            {
                new NotificationDto { Id = 1, Message = "Hello" }
            });

        var ctrl = new NotificationController(mock.Object);

        var result = await ctrl.GetForUser(1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Single((List<NotificationDto>)result.Value!);
    }

    [Fact]
    public async Task MarkRead_ShouldReturnNoContent()
    {
        var mock = new Mock<NotificationService>(null!);
        mock.Setup(s => s.MarkAsReadAsync(1)).Returns(Task.CompletedTask);

        var ctrl = new NotificationController(mock.Object);

        var result = await ctrl.MarkRead(1);

        Assert.IsType<NoContentResult>(result);
    }
}
