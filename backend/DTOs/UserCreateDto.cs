namespace backend.DTOs
{
    public class UserCreateDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
        public int RelatedId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
