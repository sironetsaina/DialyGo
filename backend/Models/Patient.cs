using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string Name { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? MedicalHistory { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Smsnotification> Smsnotifications { get; set; } = new List<Smsnotification>();

    public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; } = new List<TreatmentRecord>();

    public virtual ICollection<TruckAssignment> TruckAssignments { get; set; } = new List<TruckAssignment>();
}
