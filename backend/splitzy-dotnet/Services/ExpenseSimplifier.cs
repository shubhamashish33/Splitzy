using splitzy_dotnet.DTO;

namespace splitzy_dotnet.Services
{
    public class ExpenseSimplifier
    {
        public List<ExpensesDTO> Simplify(Dictionary<int, decimal> netBalances)
        {
            List<ExpensesDTO> result = new();

            while (true)
            {
                var maxCreditor = netBalances.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                var maxDebtor = netBalances.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

                if (Math.Abs(netBalances[maxCreditor]) < 0.01m && Math.Abs(netBalances[maxDebtor]) < 0.01m)
                    break;

                var amount = Math.Min(-netBalances[maxDebtor], netBalances[maxCreditor]);

                netBalances[maxCreditor] -= amount;
                netBalances[maxDebtor] += amount;

                result.Add(new ExpensesDTO
                {
                    FromUser = maxDebtor,
                    ToUser = maxCreditor,
                    Amount = Math.Round(amount, 2)
                });
            }

            return result;
        }
    }
}
