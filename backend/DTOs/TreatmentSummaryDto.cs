// backend/DTOs/TreatmentSummaryDto.cs
namespace backend.DTOs
{
    public class TreatmentSummaryDto
    {
        public int TreatmentId { get; set; }

        // Use DateTime? here to match your TreatmentRecord.TreatmentDate (which is DateTime)
        public DateTime? TreatmentDate { get; set; }

        public string Diagnosis { get; set; } = null!;
        public string TreatmentDetails { get; set; } = null!;
    }
}
