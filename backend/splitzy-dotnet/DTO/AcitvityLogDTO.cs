namespace splitzy_dotnet.DTO
{
    public class AcitvityLogDTO
    {
        public int ActivityLogId { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;  // AddExpense, UpdateExpense, DeleteExpense
        public string ExpenseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
