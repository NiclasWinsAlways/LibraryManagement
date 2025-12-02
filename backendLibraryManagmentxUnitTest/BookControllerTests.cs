using backendLibraryManagement.Controllers;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class BookControllerTests
{
    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenMissing()
    {
        var mock = new Mock<IBookService>();
        mock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((BookDto?)null);

        var ctrl = new BookController(mock.Object);

        var result = await ctrl.Get(5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnCreated()
    {
        var mock = new Mock<IBookService>();
        mock.Setup(s => s.CreateAsync(It.IsAny<CreateBookDto>()))
            .ReturnsAsync(new BookDto { Id = 1, Title = "T" });

        var ctrl = new BookController(mock.Object);

        var result = await ctrl.Create(new CreateBookDto()) as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(1, ((BookDto)result.Value!).Id);
    }
}
