using Xunit;
using Moq;
using backendLibraryManagement.Controllers;
using backendLibraryManagement.Services;
using backendLibraryManagement.Dto;
using Microsoft.AspNetCore.Mvc;

public class LoanControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenMissing()
    {
        var mock = new Mock<LoanService>(null!);
        mock.Setup(s => s.GetByIdAsync(9)).ReturnsAsync((LoanDto?)null);

        var ctrl = new LoanController(mock.Object);

        var result = await ctrl.Get(9);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenFails()
    {
        var mock = new Mock<LoanService>(null!);
        mock.Setup(s => s.CreateAsync(It.IsAny<CreateLoanDto>()))
            .ReturnsAsync((false, "err", null));

        var ctrl = new LoanController(mock.Object);

        var result = await ctrl.Create(new CreateLoanDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
