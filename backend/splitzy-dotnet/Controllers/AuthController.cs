using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using splitzy_dotnet.Services.Interfaces;
using System.Security.Claims;

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
        /// Sample End point for testing
        /// </summary>
        /// <returns></returns>
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return Ok("Welcome to Google SSO API!");
        }

        /// <summary>
        /// Logs in an existing user and returns a JWT token.
        /// </summary>
        /// <param name="user">Login credentials</param>
        /// <returns>JWT Token and User ID</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status500InternalServerError)]
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
        /// Endpoint for SSO Login
        /// </summary>
        /// <returns></returns>
        [HttpGet("ssologin")]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status200OK)]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status500InternalServerError)]
        public IActionResult SSOLogin()
        {
            // Edge case: If user is already authenticated, redirect to secure
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect("/secure");
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = "/secure"
            };

            // Edge case: If Google authentication scheme is not configured
            if (string.IsNullOrEmpty(GoogleDefaults.AuthenticationScheme))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Google authentication is not configured."
                });
            }

            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }
        /// <summary>
        /// Secire Endpoint for GoogleOAuth
        /// </summary>
        /// <returns></returns>
        [HttpGet("secure")]
        [Authorize(AuthenticationSchemes = "GoogleCookies", Policy = "GooglePolicy")]
        public IActionResult Secure()
        {
            // Edge case: If user is not authenticated
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User is not authenticated."
                });
            }

            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            // Edge case: If email claim is missing
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Email claim not found in user identity."
                });
            }

            var loginUser = _context.Users.FirstOrDefault(u => u.Email == email);

            // Edge case: If user is not found in database
            if (loginUser == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Success = false,
                    Message = "User not registered in the system."
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
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status400BadRequest)]
        [ProducesResponseType((typeof(ApiResponse<>)), StatusCodes.Status500InternalServerError)]
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
