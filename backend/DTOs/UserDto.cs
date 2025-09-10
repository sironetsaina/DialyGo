namespace backend.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
        public int? RelatedId { get; set; }
        public bool IsActive { get; set; }
    }
}
