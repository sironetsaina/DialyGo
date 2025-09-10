namespace backend.DTOs
{
    public class TruckDto
    {
        public int TruckId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? CurrentLocation { get; set; }
        public int? Capacity { get; set; }
    }

    public class TruckCreateDto
    {
        public string LicensePlate { get; set; } = null!;
        public string? CurrentLocation { get; set; }
        public int? Capacity { get; set; }
    }
}
