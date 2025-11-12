using Xunit;
using Moq;
using backendLibraryManagement.Controllers;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;
using Microsoft.AspNetCore.Mvc;

public class ReservationControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenMissing()
    {
        var mock = new Mock<ReservationService>(null!);
        mock.Setup(s => s.GetByIdAsync(44)).ReturnsAsync((ReservationDto?)null);

        var ctrl = new ReservationController(mock.Object);

        var result = await ctrl.Get(44);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenFails()
    {
        var mock = new Mock<ReservationService>(null!);
        mock.Setup(s => s.CreateAsync(It.IsAny<CreateReservationDto>()))
            .ReturnsAsync((false, "err", null));

        var ctrl = new ReservationController(mock.Object);

        var result = await ctrl.Create(new CreateReservationDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
