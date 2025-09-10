using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TruckAssignment
{
    public int AssignmentId { get; set; }

    public int? TruckId { get; set; }

    public int? PatientId { get; set; }

    public DateOnly? DateAssigned { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Truck? Truck { get; set; }
}
