namespace backend.DTOs;
public class TreatmentEntryDto
{
    public string Diagnosis { get; set; } = string.Empty;
    public string TreatmentDetails { get; set; } = string.Empty;
    public DateTime TreatmentDate { get; set; } = DateTime.UtcNow;
}