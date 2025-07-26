using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using System.Text.RegularExpressions;

namespace splitzy_dotnet.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly SplitzyContext _context;
        public DashboardController(SplitzyContext context)
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
            // Get groupIds where user is a member
            var groupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            // 1️⃣ Fetch recent expenses in those groups
            var expenses = await _context.Expenses
                .Include(e => e.ExpenseSplits)
                .Include(e => e.Group)
                .Where(e => groupIds.Contains(e.GroupId))
                .OrderByDescending(e => e.CreatedAt)
                .Take(20)
                .ToListAsync();

            var activities = new List<RecentActivityDTO>();

            foreach (var expense in expenses)
            {
                var payerName = await _context.Users
                    .Where(u => u.UserId == expense.PaidByUserId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync() ?? "Someone";

                var userSplit = expense.ExpenseSplits
                    .FirstOrDefault(s => s.UserId == userId)?.OwedAmount ?? 0;

                var impactAmount = expense.PaidByUserId == userId
                    ? expense.Amount - userSplit
                    : -userSplit;

                activities.Add(new RecentActivityDTO
                {
                    Actor = expense.PaidByUserId == userId ? "You" : payerName,
                    Action = "added",
                    ExpenseName = expense.Name,
                    GroupName = expense.Group?.Name ?? "",
                    CreatedAt = expense.CreatedAt ?? DateTime.MinValue, // Fix CS8629
                    Impact = new ActivityImpact
                    {
                        Type = impactAmount >= 0 ? "get_back" : "owe",
                        Amount = Math.Abs(impactAmount)
                    }
                });
            }

            // 2️⃣ Fetch activity logs in those groups (including update/delete by anyone)
            var logs = await _context.ActivityLogs
                .Where(a => groupIds.Contains(a.GroupId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .ToListAsync();

            foreach (var log in logs)
            {
                if (log.ActionType == "AddExpense") continue; // Skip "added" logs to avoid duplication

                // Get actor name from UserId
                string actorName;
                if (log.UserId == userId)
                {
                    actorName = "You";
                }
                else
                {
                    actorName = await _context.Users
                        .Where(u => u.UserId == log.UserId)
                        .Select(u => u.Name)
                        .FirstOrDefaultAsync() ?? "Someone";
                }

                // Get group name from GroupId
                var groupName = await _context.Groups
                    .Where(g => g.GroupId == log.GroupId)
                    .Select(g => g.Name)
                    .FirstOrDefaultAsync() ?? "";

                activities.Add(new RecentActivityDTO
                {
                    Actor = actorName,
                    Action = log.ActionType switch
                    {
                        "UpdateExpense" => "updated",
                        "DeleteExpense" => "deleted",
                        _ => log.ActionType.ToLower()
                    },
                    ExpenseName = log.Expense.Name,
                    GroupName = groupName,
                    CreatedAt = log.CreatedAt,
                    Impact = new ActivityImpact
                    {
                        Type = "info",
                        Amount = log.Expense.Amount
                    }
                });
            }

            // 3️⃣ Sort all activities by date and take top 10
            var sortedActivities = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToList();

            return Ok(sortedActivities);
        }

    }
}
