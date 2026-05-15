using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopSchedule
{
    public int Id { get; set; }

    public int WorkshopId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool CreatedFromRepeat { get; set; }

    public virtual Workshop Workshop { get; set; } = null!;

    public virtual ICollection<WorkshopTicket> WorkshopTickets { get; set; } = new List<WorkshopTicket>();
}
