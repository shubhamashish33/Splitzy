namespace splitzy_dotnet.DTO
{
    public class SettlementDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PaidBy { get; set; }
        public int PaidTo { get; set; }
        public decimal Amount { get; set; }
    }
}
