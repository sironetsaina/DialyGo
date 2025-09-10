namespace backend.DTOs
{
    public class DoctorCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Specialization { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
