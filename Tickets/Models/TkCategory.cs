using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class TkCategory
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}