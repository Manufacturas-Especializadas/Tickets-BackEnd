using System;
using System.Collections.Generic;

namespace Tickets.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int? StatusId { get; set; }

    public string Name { get; set; }

    public string Department { get; set; }

    public string Affair { get; set; }

    public string ProblemDescription { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual TkCategory Category { get; set; }

    public virtual TkStatus Status { get; set; }
}