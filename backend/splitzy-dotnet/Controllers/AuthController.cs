using Microsoft.AspNetCore.Mvc;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using splitzy_dotnet.Services.Interfaces;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJWTService _jWTService;

        public AuthController(AppDbContext context, IJWTService jWTService)
        {
            _context = context;
            _jWTService = jWTService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO user)
        {
            var loginUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (loginUser == null)
            {
                return Unauthorized("User Not Found");
            }

            if (loginUser.PasswordHash != user.Password)
            {
                return Unauthorized("Password is Wrong");
            }

            var token = _jWTService.GenerateToken(loginUser.UserId);

            return Ok(new
            {
                Message = "Login Succesfully",
                Id = loginUser.UserId,
                Token = token
            });
        }
    }
}
