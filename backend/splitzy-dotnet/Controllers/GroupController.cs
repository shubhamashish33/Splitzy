using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using static splitzy_dotnet.DTO.GroupDTO;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllGroupByUser/{userId}")]
        public async Task<IEnumerable<UserGroupInfo>> GetAllGroupByUser(int userId)
        {
            var groupMemberships = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .ToListAsync();

            var result = groupMemberships.Select(gm => new UserGroupInfo
            {
                GroupId = gm.GroupId,
                GroupName = gm.Group.Name,
                JoinedAt = gm.JoinedAt
            });

            return result;
        }

        [HttpGet("GetGroupSummary/{groupId}")]
        public async Task<ActionResult<GroupSummaryDTO>> GetGroupSummary(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .Include(g => g.Expenses)
                    .ThenInclude(e => e.PaidByUser)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            if (group == null)
                return NotFound();

            var usernames = group.GroupMembers.Select(gm => gm.User.Name).ToList();

            var expenses = group.Expenses.Select(e => new GroupExpenseDTO
            {
                PaidBy = e.PaidByUser.Name,
                Name = e.Name,
                Amount = e.Amount
            }).ToList();

            var summary = new GroupSummaryDTO
            {
                GroupId = group.GroupId,
                GroupName = group.Name,
                TotalMembers = usernames.Count,
                Usernames = usernames,
                Expenses = expenses
            };

            return Ok(summary);
        }


        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            // Validate users exist
            var users = await _context.Users
                .Where(u => request.UserIds.Contains(u.UserId))
                .ToListAsync();

            if (users.Count != request.UserIds.Count)
                return NotFound("One or more users not found.");

            // Create new group
            var group = new Group
            {
                Name = request.GroupName,
                CreatedAt = DateTime.UtcNow
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add all users as group members
            var groupMembers = users.Select(u => new GroupMember
            {
                GroupId = group.GroupId,
                UserId = u.UserId,
                JoinedAt = DateTime.UtcNow
            }).ToList();

            _context.GroupMembers.AddRange(groupMembers);
            await _context.SaveChangesAsync();

            // Prepare response with member details
            var memberDetails = users.Select(u => new
            {
                u.UserId,
                u.Name,
                u.Email
            }).ToList();

            return Ok(new
            {
                GroupId = group.GroupId,
                GroupName = group.Name,
                CreatedAt = group.CreatedAt,
                Members = memberDetails
            });
        }
    }
}
