namespace ImbUserManagment2.ViewModels.Api
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Department { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public IList<string> Roles { get; set; } = new List<string>();
    }
}