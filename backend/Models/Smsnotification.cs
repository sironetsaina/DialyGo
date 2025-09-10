using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Smsnotification
{
    public int SmsId { get; set; }

    public int PatientId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public string? SentBy { get; set; }

    public int? SenderId { get; set; }

    public string? SenderRole { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
