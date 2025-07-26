using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;

namespace splitzy_dotnet.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("dashboard/{userId}")]
        public async Task<ActionResult<UserDTO>> GetDashboard(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // Get all groups user is in
            var groupMemberships = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .ToListAsync();

            // Total paid
            var totalPaid = await _context.Expenses
                .Where(e => e.PaidByUserId == userId)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            // Total owed by user (splits)
            var youOwe = await _context.ExpenseSplits
                .Where(s => s.UserId == userId)
                .SumAsync(s => (decimal?)s.OwedAmount) ?? 0;

            // Total owed to user
            var youAreOwed = await _context.Expenses
                .Where(e => e.PaidByUserId == userId)
                .Join(
                    _context.ExpenseSplits,
                    e => e.ExpenseId,
                    s => s.ExpenseId,
                    (e, s) => new { e.PaidByUserId, s.UserId, s.OwedAmount }
                )
                .Where(x => x.UserId != userId)
                .SumAsync(x => (decimal?)x.OwedAmount) ?? 0;

            // List of people user owes to
            var oweTo = await _context.ExpenseSplits
                .Where(s => s.UserId == userId)
                .Join(_context.Expenses,
                      split => split.ExpenseId,
                      expense => expense.ExpenseId,
                      (split, expense) => new { split, expense })
                .Where(x => x.expense.PaidByUserId != userId)
                .GroupBy(x => x.expense.PaidByUserId)
                .Select(g => new PersonAmount
                {
                    Name = _context.Users.Where(u => u.UserId == g.Key).Select(u => u.Name).FirstOrDefault(),
                    Amount = g.Sum(x => x.split.OwedAmount)
                })
                .ToListAsync();

            // List of people who owe user
            var owedFrom = await _context.Expenses
                .Where(e => e.PaidByUserId == userId)
                .Join(_context.ExpenseSplits,
                      e => e.ExpenseId,
                      s => s.ExpenseId,
                      (e, s) => new { e, s })
                .Where(x => x.s.UserId != userId)
                .GroupBy(x => x.s.UserId)
                .Select(g => new PersonAmount
                {
                    Name = _context.Users.Where(u => u.UserId == g.Key).Select(u => u.Name).FirstOrDefault(),
                    Amount = g.Sum(x => x.s.OwedAmount)
                })
                .ToListAsync();

            // Group-wise summary
            var groupWiseSummary = await _context.Groups
                .Where(g => groupMemberships.Select(m => m.GroupId).Contains(g.GroupId))
                .Select(g => new GroupSummary
                {
                    GroupId = g.GroupId,
                    GroupName = g.Name,
                    NetBalance = (
                        _context.Expenses
                            .Where(e => e.GroupId == g.GroupId && e.PaidByUserId == userId)
                            .Join(_context.ExpenseSplits,
                                  e => e.ExpenseId,
                                  s => s.ExpenseId,
                                  (e, s) => new { e, s })
                            .Where(x => x.s.UserId != userId)
                            .Sum(x => (decimal?)x.s.OwedAmount) ?? 0
                    ) - (
                        _context.ExpenseSplits
                            .Where(s => s.UserId == userId)
                            .Join(_context.Expenses,
                                  s => s.ExpenseId,
                                  e => e.ExpenseId,
                                  (s, e) => new { s, e })
                            .Where(x => x.e.GroupId == g.GroupId)
                            .Sum(x => (decimal?)x.s.OwedAmount) ?? 0
                    )
                }).ToListAsync();

            var result = new UserDTO
            {
                UserId = user.UserId,
                UserName = user.Name,
                TotalBalance = youAreOwed - youOwe,
                YouOwe = youOwe,
                YouAreOwed = youAreOwed,
                OweTo = oweTo,
                OwedFrom = owedFrom,
                GroupWiseSummary = groupWiseSummary
            };

            return Ok(result);
        }

        [HttpGet("recent/{userId}")]
        public async Task<IActionResult> GetRecentActivity(int userId)
        {
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseSplits)
                .Include(e => e.Group)
                .Where(e => e.ExpenseSplits.Any(s => s.UserId == userId) || e.PaidByUserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .ToListAsync();

            var activities = new List<RecentActivityDTO>();

            foreach (var expense in expenses)
            {
                var actor = expense.PaidByUserId == userId ? "You" : _context.Users.Find(expense.PaidByUserId)?.Name ?? "Someone";
                var userSplit = expense.ExpenseSplits.FirstOrDefault(s => s.UserId == userId)?.OwedAmount ?? 0;
                var impactAmount = expense.PaidByUserId == userId ? expense.Amount - userSplit : -userSplit;

                activities.Add(new RecentActivityDTO
                {
                    Actor = actor,
                    Action = "added",
                    ExpenseName = expense.Name,
                    GroupName = expense.Group?.Name ?? "",
                    CreatedAt = (DateTime)expense.CreatedAt,
                    Impact = new ActivityImpact
                    {
                        Type = impactAmount >= 0 ? "get_back" : "owe",
                        Amount = Math.Abs(impactAmount)
                    }
                });
            }

            return Ok(activities);
        }

    }
}
