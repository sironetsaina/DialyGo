using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Truck
{
    public int TruckId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? CurrentLocation { get; set; }

    public int? Capacity { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<TruckAssignment> TruckAssignments { get; set; } = new List<TruckAssignment>();

    public virtual ICollection<TruckMaintenance> TruckMaintenances { get; set; } = new List<TruckMaintenance>();

    public virtual ICollection<TruckStaffAssignment> TruckStaffAssignments { get; set; } = new List<TruckStaffAssignment>();
}
