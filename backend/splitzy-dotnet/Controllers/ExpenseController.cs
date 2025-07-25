using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using splitzy_dotnet.DTO;
using splitzy_dotnet.Models;
using splitzy_dotnet.Services;
using System.Text.Json;

namespace splitzy_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddExpense")]
        public async Task<IActionResult> AddExpense([FromBody] CreateExpenseDto dto)
        {
            var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == dto.GroupId);
            if (!groupExists)
                return BadRequest("Invalid group");

            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == dto.GroupId && gm.UserId == dto.PaidByUserId);
            if (!isMember)
                return BadRequest("Payer must be a member of the group");

            Dictionary<int, decimal> finalSplits = new();

            if (dto.SplitType == SplitType.Equal)
            {
                var groupUserIds = await _context.GroupMembers
                    .Where(gm => gm.GroupId == dto.GroupId)
                    .Select(gm => gm.UserId).ToListAsync();

                decimal equalSplit = Math.Round(1.0m / groupUserIds.Count, 4);
                foreach (var uid in groupUserIds)
                    finalSplits[uid] = equalSplit;
            }
            else if (dto.SplitType == SplitType.Percentage || dto.SplitType == SplitType.Exact)
            {
                finalSplits = dto.SplitDetails;
                var total = dto.SplitType == SplitType.Percentage ? finalSplits.Values.Sum() : finalSplits.Values.Sum() / dto.Amount;
                if (Math.Abs(total - 1.0m) > 0.01m)
                    return BadRequest("Split percentages or amounts must total to 100% of the expense");
            }

            var expense = new Expense
            {
                Name = dto.Name,
                Amount = dto.Amount,
                GroupId = dto.GroupId,
                PaidByUserId = dto.PaidByUserId,
                SplitPer = JsonSerializer.Serialize(finalSplits),
                CreatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var splits = finalSplits.Select(pair => new ExpenseSplit
            {
                ExpenseId = expense.ExpenseId,
                UserId = pair.Key,
                OwedAmount = dto.SplitType == SplitType.Exact ? pair.Value : Math.Round(dto.Amount * pair.Value, 2)
            });

            _context.ExpenseSplits.AddRange(splits);
            await _context.SaveChangesAsync();

            var groupUserIdsFinal = await _context.GroupMembers
                .Where(gm => gm.GroupId == dto.GroupId)
                .Select(gm => gm.UserId)
                .ToListAsync();

            var netBalances = groupUserIdsFinal.ToDictionary(uid => uid, uid => 0.0m);

            var allExpenses = await _context.Expenses
                .Where(e => e.GroupId == dto.GroupId)
                .Include(e => e.ExpenseSplits)
                .ToListAsync();

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

            var simplifier = new ExpenseSimplifier();
            var simplifiedTransactions = simplifier.Simplify(netBalances);

            return Ok(new
            {
                message = "Expense added successfully",
                expenseId = expense.ExpenseId,
                simplifiedTransactions
            });
        }
    }
}