namespace splitzy_dotnet.DTO
{
    public class ExpenseDTO
    {
        public int ExpenseId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Amount { get; set; }
        public int GroupId { get; set; }
        public int PaidByUserId { get; set; }
        public Dictionary<string, decimal> SplitPer { get; set; } = new();
    }
}
