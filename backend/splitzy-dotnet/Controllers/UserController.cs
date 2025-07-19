using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using splitzy_dotnet.Services.Interfaces;

namespace splitzy_dotnet.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJWTService _jWTService;
        public UserController(AppDbContext context, IJWTService jWTService)
        {
            _context = context;
            _jWTService = jWTService;   
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAll()
        {
            //var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            //if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            //{
            //    return Unauthorized("No token provided.");
            //}

            //var token = authHeader.Substring("Bearer ".Length).Trim();

            //// Validate the token using your JWT service
            //var isValid = _jWTService.ValidateToken(token);
            //if (!isValid)
            //{
            //    return Unauthorized("Invalid token.");
            //}
            var usersFromDB = _context.Users.ToList();

            var users = usersFromDB.Select(u => new UserDTO
            {
                Name = u.Name,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            }).ToList();

            return Ok(users);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserGroupExpenseDTO>> GetUserGroupSummary(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            // Get groups the user has joined
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Select(gm => new UserGroupInfo
                {
                    GroupId = gm.GroupId,
                    GroupName = gm.Group.Name,
                    JoinedAt = gm.JoinedAt
                }).ToListAsync();

            // Get total paid by user from Expenses table
            var totalPaid = await _context.Expenses
                .Where(e => e.PaidByUserId == userId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            return Ok(new UserGroupExpenseDTO
            {
                UserId = user.UserId,
                UserName = user.Name,
                Groups = groups,
                TotalPaid = totalPaid
            });
        }
    }
}
