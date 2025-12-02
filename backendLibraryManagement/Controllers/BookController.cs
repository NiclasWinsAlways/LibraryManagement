using backendLibraryManagement.Dto;
using backendLibraryManagement.Services;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _svc;
        public BookController(IBookService svc) => _svc = svc;

        // GET: api/Book/getbooks
        // Returns all books in the system.
        [HttpGet("getbooks")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        // GET: api/Book/{id}
        // Returns a single book by ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var book = await _svc.GetByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        // POST: api/Book/create
        // Creates a new book entry.
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateBookDto dto)
        {
            var book = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = book.Id }, book);
        }

        // PUT: api/Book/{id}
        // Updates an existing book.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto)
        {
            var ok = await _svc.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE: api/Book/{id}
        // Deletes a book by ID.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
