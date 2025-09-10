namespace backend.DTOs
{
    public class TruckStaffAssignmentDto
    {
        public int AssignmentId { get; set; }
        public int TruckId { get; set; }
        public int StaffId { get; set; }
        public string Role { get; set; } = null!;
        public DateOnly DateAssigned { get; set; }
    }

    public class TruckStaffAssignmentCreateDto
    {
        public int TruckId { get; set; }
        public int StaffId { get; set; }
        public string Role { get; set; } = null!;
    }
}
