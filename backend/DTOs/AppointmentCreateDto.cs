namespace backend.DTOs
{
    public class AppointmentCreateDto
    {
        public int PatientId { get; set; }
        public int TruckId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }

    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? TruckPlate { get; set; }
    }

    public class PatientDetailDto
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistory { get; set; }
    }

    public class TruckLocationDto
    {
        public int TruckId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? CurrentLocation { get; set; }
    }
}
