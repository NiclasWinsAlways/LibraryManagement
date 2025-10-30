using backendLibraryManagement.Dto;
using backendLibraryManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace backendLibraryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) => _auth = auth;

        // POST api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null) return BadRequest();
            var (success, token, error) = await _auth.AuthenticateAsync(dto.Email, dto.Password);
            if (!success) return Unauthorized(new { error });
            return Ok(new { token });
        }
    }
}