using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int PayRollNumber { get; set; }

    public int RolId { get; set; }

    public string? PasswordHash { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public virtual Role Rol { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}