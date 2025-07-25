namespace splitzy_dotnet.DTO
{
    public class ExpensesDTO
    {
        public int FromUser { get; set; }
        public int ToUser { get; set; }
        public decimal Amount { get; set; }
    }
    public enum SplitType
    {
        Equal,
        Percentage,
        Exact
    }

    public class CreateExpenseDto
    {
        public required string Name { get; set; }
        public decimal Amount { get; set; }
        public int GroupId { get; set; }
        public int PaidByUserId { get; set; }
        public SplitType SplitType { get; set; }
        public required Dictionary<int, decimal> SplitDetails { get; set; }
    }
}
