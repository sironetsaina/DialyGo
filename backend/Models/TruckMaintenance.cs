using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class TruckMaintenance
{
    public int MaintenanceId { get; set; }

    public int? TruckId { get; set; }

    public string? Description { get; set; }

    public DateOnly? MaintenanceDate { get; set; }

    public DateOnly? NextServiceDue { get; set; }

    public virtual Truck? Truck { get; set; }
}
