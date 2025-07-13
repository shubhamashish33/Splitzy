namespace splitzy_dotnet.DTO
{
    public class UserDTO
    {
        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }
    }
}
