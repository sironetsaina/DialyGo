using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public int RelatedId { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<LoginSession> LoginSessions { get; set; } = new List<LoginSession>();

    public virtual UserRole Role { get; set; } = null!;
}
