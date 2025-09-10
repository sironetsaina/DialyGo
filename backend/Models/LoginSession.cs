using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class LoginSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public DateTime? LoginTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public virtual User User { get; set; } = null!;
}
