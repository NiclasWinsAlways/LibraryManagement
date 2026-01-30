using backendLibraryManagement.Dto;
using backendLibraryManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _svc;
        public UserController(IUserService svc) => _svc = svc;

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _svc.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // NEW: get current user by JWT sub claim
        // GET: api/User/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserIdFromJwt();
            if (userId == null) return Unauthorized(new { error = "Invalid token" });

            var user = await _svc.GetByIdAsync(userId.Value);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // NEW: update current user profile
        // PUT: api/User/me
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMyProfileDto dto)
        {
            var userId = GetUserIdFromJwt();
            if (userId == null) return Unauthorized(new { error = "Invalid token" });

            // Map to UpdateUserDto (role not allowed here)
            var mapped = new UpdateUserDto
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                PhoneNumber = dto.PhoneNumber,
                SmsOptIn = dto.SmsOptIn,
                EmailOptIn = dto.EmailOptIn,
                Role = null
            };

            var (success, error) = await _svc.UpdateAsync(userId.Value, mapped, allowRoleChange: false);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    "EmailExists" => Conflict(new { error = "Email already in use" }),
                    _ => BadRequest(new { error })
                };
            }

            return NoContent();
        }

        // Existing update by id (admin role changes)
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var wantsToChangeRole = !string.IsNullOrWhiteSpace(dto.Role);

            if (wantsToChangeRole)
            {
                if (!(User?.Identity?.IsAuthenticated ?? false)) return Forbid();
                if (!User.IsInRole("Admin")) return Forbid();
            }

            var (success, error) = await _svc.UpdateAsync(id, dto, allowRoleChange: wantsToChangeRole);
            if (!success)
            {
                return error switch
                {
                    "NotFound" => NotFound(),
                    "EmailExists" => Conflict(new { error = "Email already in use" }),
                    _ => BadRequest(new { error })
                };
            }

            return NoContent();
        }

        private int? GetUserIdFromJwt()
        {
            // token has sub as userId (AuthService sets JwtRegisteredClaimNames.Sub)
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(sub, out var id)) return id;
            return null;
        }
    }
}
