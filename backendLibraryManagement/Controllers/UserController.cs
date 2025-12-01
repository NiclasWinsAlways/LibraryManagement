using backendLibraryManagement.Dto;
using backendLibraryManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _svc;
        public UserController(UserService svc) => _svc = svc;

        // GET: api/User/GetUsers
        // Returns all registered users.
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        // GET: api/User/{id}
        // Returns a specific user by ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _svc.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST: api/User/create
        // Creates a new user account.
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateUserDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        // PUT: api/User/{id}
        // Updates a user profile. Role changes require admin privileges.
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult>Update(int id,UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Detect role change attempt.
            var wantsToChangeRole = !string.IsNullOrWhiteSpace(dto.Role);

            // Require admin if role is being changed.
            if (wantsToChangeRole)
            {
                //if not authenticated or not admin role, frobid role change
                if (!(User?.Identity?.IsAuthenticated ?? false))
                    return Forbid();
                if (!User.IsInRole("Admin"))
                    return Forbid();
            }
            var (success, error) = await _svc.UpdateAsync(id, dto, allowRoleChange: wantsToChangeRole);
            if (!success)
            {
                return error switch
                {
                    "NotFoudnd" => NotFound(),
                    "EmailExists" => Conflict(new { error = "Email already in use" }),
                    _ => BadRequest(new { error })
                };
            }
            return NoContent();
        }
    }
}