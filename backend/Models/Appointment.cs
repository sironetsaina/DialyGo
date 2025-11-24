using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int PatientId { get; set; }


    public int? NurseId { get; set; }

    public int? TruckId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }


    public virtual Nurse? Nurse { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; } = new List<TreatmentRecord>();

    public virtual Truck? Truck { get; set; }

}





