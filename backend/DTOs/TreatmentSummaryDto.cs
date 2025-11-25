// backend/DTOs/TreatmentSummaryDto.cs
namespace backend.DTOs
{
    public class TreatmentSummaryDto
    {
        public int TreatmentId { get; set; }

        public DateTime? TreatmentDate { get; set; }

        public string Diagnosis { get; set; } = null!;
        public string TreatmentDetails { get; set; } = null!;
        public int? DoctorId { get; internal set; }
    }
    

}
