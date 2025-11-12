using backendLibraryManagement.Data;
using backendLibraryManagement.Dto;
using backendLibraryManagement.Model;
using backendLibraryManagement.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;


public class BookServiceTests
{
    private LibraryContext GetDb()
    {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryContext(options);
    }
    [Fact]
    public async Task CreateAsync_ShouldCreateBook()
    {
        var db = GetDb();
        var svc = new BookService(db);
        var dto = new CreateBookDto
        {
            Title = "Test",
            Genre = "Fantasy",
            Author = "Author",
            ISBN = "123"
        };
        var result = await svc.CreateAsync(dto);
        Assert.Equal("Test", result.Title);
        Assert.Single(db.Books);
    }
    [Fact]
    public async Task GetById_ShouldReturnNull_WhenNotExists()
    {
        var db = GetDb();
        var svc = new BookService(db);
        var book = await svc.GetByIdAsync(100);
        Assert.Null(book);
    }
    public async Task DeleteAsync_ShouldRemoveBook()
    {
        var db = GetDb();
        db.Books.Add(new Book { Id = 1, Title = "Test" });
        await db.SaveChangesAsync();
        var svc = new BookService(db);
        var ok = await svc.DeleteAsync(1);
        Assert.True(ok);
        Assert.Empty(db.Books);
    }
}
