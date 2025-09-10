using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Userdetailsview
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public int RelatedId { get; set; }

    public string? FullName { get; set; }

    public string? Specialization { get; set; }

    public string? Email { get; set; }
}
