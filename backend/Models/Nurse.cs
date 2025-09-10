using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Nurse
{
    public int NurseId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
