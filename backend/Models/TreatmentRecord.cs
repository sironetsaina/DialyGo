using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TreatmentRecord
{
    public int TreatmentId { get; set; }

    public int AppointmentId { get; set; }

    public int PatientId { get; set; }

    public string? Diagnosis { get; set; }

    public string? TreatmentDetails { get; set; }

    public DateTime? TreatmentDate { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;
}
