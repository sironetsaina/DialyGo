// backend/DTOs/PatientDtos.cs
namespace backend.DTOs
{
    public class PatientCreateDto
    {
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
    }

    public class PatientUpdateDto : PatientCreateDto { }

    public class PatientDto
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }
}
