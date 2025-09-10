using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Appointmentdetail
{
    public int AppointmentId { get; set; }

    public string PatientName { get; set; } = null!;

    public string? DoctorName { get; set; }

    public string? NurseName { get; set; }

    public string? TruckPlate { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }
}
