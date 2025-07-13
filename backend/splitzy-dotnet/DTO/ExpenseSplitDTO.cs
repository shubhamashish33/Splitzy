namespace splitzy_dotnet.DTO
{
    public class ExpenseSplitDTO
    {
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public int UserId { get; set; }
        public decimal OwedAmount { get; set; }
    }
}
