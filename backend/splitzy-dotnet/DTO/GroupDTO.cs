namespace splitzy_dotnet.DTO
{
    public class GroupDTO
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
    }
}
