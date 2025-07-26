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
        private readonly SplitzyContext _context;
        public GroupController(SplitzyContext context)
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

        [HttpGet("GetGroupOverview/{userId}/{groupId}")]
        public async Task<IActionResult> GetGroupOverview(int userId, int groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId);
            if (group == null) return NotFound("Group not found");

            var members = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Select(gm => gm.UserId)
                .ToListAsync();

            var userNameMap = await _context.Users
                .Where(u => members.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Name);

            var allExpenses = await _context.Expenses
                .Where(e => e.GroupId == groupId)
                .Include(e => e.ExpenseSplits)
                .ToListAsync();

            var netBalances = members.ToDictionary(id => id, id => 0.0m);

            foreach (var exp in allExpenses)
            {
                foreach (var split in exp.ExpenseSplits)
                {
                    if (netBalances.ContainsKey(split.UserId))
                        netBalances[split.UserId] -= split.OwedAmount;
                }

                if (netBalances.ContainsKey(exp.PaidByUserId))
                    netBalances[exp.PaidByUserId] += exp.Amount;
            }

            decimal groupBalance = netBalances.Sum(nb => nb.Value);
            decimal youOwe = netBalances[userId] < 0 ? Math.Abs(netBalances[userId]) : 0;
            decimal youAreOwed = netBalances[userId] > 0 ? netBalances[userId] : 0;

            var expenses = allExpenses.Select(e => new
            {
                e.ExpenseId,
                e.Name,
                e.Amount,
                PaidBy = userNameMap[e.PaidByUserId],
                CreatedAt = e.CreatedAt?.ToString("MMM dd") ?? string.Empty,
                YouOwe = e.ExpenseSplits.FirstOrDefault(s => s.UserId == userId)?.OwedAmount ?? 0
            });

            // Return net balance summary for all users
            var allUserSummaries = netBalances.Select(kvp => new
            {
                UserId = kvp.Key,
                Name = userNameMap[kvp.Key],
                Balance = Math.Round(kvp.Value, 2),
            }).ToList();

            return Ok(new
            {
                group.GroupId,
                group.Name,
                Created = group.CreatedAt?.ToString("MMM dd") ?? string.Empty,
                GroupBalance = Math.Round(groupBalance, 2),
                MembersCount = members.Count,
                Expenses = expenses,
                Balances = new
                {
                    TotalBalance = Math.Round(netBalances[userId], 2),
                    YouOwe = Math.Round(youOwe, 2),
                    YouAreOwed = Math.Round(youAreOwed, 2)
                },
                Members = userNameMap.Select(kvp => new { MemberId = kvp.Key, MemberName = kvp.Value }).ToList(),
                UserSummaries = allUserSummaries
            });
        }
    }
}
