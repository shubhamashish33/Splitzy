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
        private readonly SplitzyContext _context;
        public ExpenseController(SplitzyContext context)
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

            if (dto.SplitDetails == null || dto.SplitDetails.Count == 0)
                return BadRequest("Split details required");

            var totalSplit = dto.SplitDetails.Sum(s => s.Amount);
            if (Math.Abs(totalSplit - dto.Amount) > 0.01m)
                return BadRequest("Split amounts must total to the expense amount");

            var splitDict = dto.SplitDetails.ToDictionary(s => s.UserId, s => s.Amount);

            var expense = new Expense
            {
                Name = dto.Name,
                Amount = dto.Amount,
                GroupId = dto.GroupId,
                PaidByUserId = dto.PaidByUserId,
                SplitPer = JsonSerializer.Serialize(splitDict),
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            var splits = dto.SplitDetails.Select(s => new ExpenseSplit
            {
                ExpenseId = expense.ExpenseId,
                UserId = s.UserId,
                OwedAmount = s.Amount
            });

            _context.ExpenseSplits.AddRange(splits);
            await _context.SaveChangesAsync();

            var groupUserIds = await _context.GroupMembers
                .Where(gm => gm.GroupId == dto.GroupId)
                .Select(gm => gm.UserId)
                .ToListAsync();

            var netBalances = groupUserIds.ToDictionary(uid => uid, uid => 0.0m);

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

            var settlements = simplifiedTransactions.Select(txn => new Settlement
            {
                GroupId = dto.GroupId,
                PaidBy = txn.FromUser,
                PaidTo = txn.ToUser,
                Amount = txn.Amount,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });

            _context.Settlements.AddRange(settlements);
            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                GroupId = dto.GroupId,
                UserId = dto.PaidByUserId,
                ActionType = "AddExpense",
                Expense = expense,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Expense added successfully",
                expenseId = expense.ExpenseId,
                simplifiedTransactions
            });
        }

        [HttpDelete("DeleteExpense/{expenseId}")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var expense = await _context.Expenses
                .Include(e => e.ExpenseSplits)
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId);

            if (expense == null)
                return NotFound("Expense not found");

            var groupId = expense.GroupId;

            _context.ExpenseSplits.RemoveRange(expense.ExpenseSplits);
            _context.Expenses.Remove(expense);

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                GroupId = groupId,
                UserId = expense.PaidByUserId,
                ActionType = "DeleteExpense",
                Expense = expense,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });
            await _context.SaveChangesAsync();

            // Recalculate simplified settlements for the group
            await RecalculateSettlementsAsync(groupId);

            return Ok(new { message = "Expense deleted successfully" });
        }
        [HttpPut("UpdateExpense")]
        public async Task<IActionResult> UpdateExpense([FromBody] UpdateExpenseDto dto)
        {
            var expense = await _context.Expenses
                .Include(e => e.ExpenseSplits)
                .FirstOrDefaultAsync(e => e.ExpenseId == dto.ExpenseId);

            if (expense == null)
                return NotFound("Expense not found");

            var groupExists = await _context.Groups.AnyAsync(g => g.GroupId == dto.GroupId);
            if (!groupExists)
                return BadRequest("Invalid group");

            if (dto.SplitDetails == null || dto.SplitDetails.Count == 0)
                return BadRequest("Split details required");

            var totalSplit = dto.SplitDetails.Sum(s => s.Amount);
            if (Math.Abs(totalSplit - dto.Amount) > 0.01m)
                return BadRequest("Split amounts must total to the expense amount");

            // Update fields
            expense.Name = dto.Name;
            expense.Amount = dto.Amount;
            expense.PaidByUserId = dto.PaidByUserId;
            expense.SplitPer = JsonSerializer.Serialize(dto.SplitDetails.ToDictionary(s => s.UserId, s => s.Amount));

            // Remove old splits
            _context.ExpenseSplits.RemoveRange(expense.ExpenseSplits);

            // Add new splits
            var newSplits = dto.SplitDetails.Select(s => new ExpenseSplit
            {
                ExpenseId = dto.ExpenseId,
                UserId = s.UserId,
                OwedAmount = s.Amount
            });
            _context.ExpenseSplits.AddRange(newSplits);

            await _context.SaveChangesAsync();

            await RecalculateSettlementsAsync(dto.GroupId);

            _context.ActivityLogs.Add(new ActivityLog
            {
                GroupId = dto.GroupId,
                UserId = dto.PaidByUserId,
                ActionType = "UpdateExpense",
                Expense = expense,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Expense updated successfully" });
        }

        private async Task RecalculateSettlementsAsync(int groupId)
        {
            var groupUserIds = await _context.GroupMembers
                .Where(gm => gm.GroupId == groupId)
                .Select(gm => gm.UserId)
                .ToListAsync();

            var netBalances = groupUserIds.ToDictionary(uid => uid, uid => 0.0m);

            var allExpenses = await _context.Expenses
                .Where(e => e.GroupId == groupId)
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

            // Remove old settlements
            var oldSettlements = await _context.Settlements
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
            _context.Settlements.RemoveRange(oldSettlements);

            var simplifier = new ExpenseSimplifier();
            var simplifiedTransactions = simplifier.Simplify(netBalances);

            var newSettlements = simplifiedTransactions.Select(txn => new Settlement
            {
                GroupId = groupId,
                PaidBy = txn.FromUser,
                PaidTo = txn.ToUser,
                Amount = txn.Amount,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });

            _context.Settlements.AddRange(newSettlements);
            await _context.SaveChangesAsync();
        }
    }
}