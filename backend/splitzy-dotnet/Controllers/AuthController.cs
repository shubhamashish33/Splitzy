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
        private readonly SplitzyContext _context;
        private readonly IJWTService _jWTService;

        public AuthController(SplitzyContext context, IJWTService jWTService)
        {
            _context = context;
            _jWTService = jWTService;
        }

        /// <summary>
        /// Logs in an existing user and returns a JWT token.
        /// </summary>
        /// <param name="user">Login credentials</param>
        /// <returns>JWT Token and User ID</returns>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO user)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid request"
                });

            var loginUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (loginUser == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            if (loginUser.PasswordHash != user.Password)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Password is incorrect"
                });
            }

            var token = _jWTService.GenerateToken(loginUser.UserId);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Login successful",
                Data = new
                {
                    Id = loginUser.UserId,
                    Token = token
                }
            });
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">User registration data</param>
        /// <returns>Created User ID</returns>
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid input"
                });

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Name, Email, and Password are required."
                });
            }

            if (_context.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Email already exists."
                });
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = request.Password,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Signup), new ApiResponse<object>
            {
                Success = true,
                Message = "Signup successful",
                Data = new { Id = user.UserId }
            });
        }
    }
}
