namespace backend.DTOs
{
    public class NurseCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }
}
