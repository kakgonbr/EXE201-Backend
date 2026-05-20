using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopTicket
{
    public int Id { get; set; }

    public string TicketType { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int WorkshopScheduleId { get; set; }

    public int MaxTickets { get; set; }

    public decimal Price { get; set; }

    public virtual ICollection<WorkshopParticipant> WorkshopParticipants { get; set; } = new List<WorkshopParticipant>();

    public virtual WorkshopSchedule WorkshopSchedule { get; set; } = null!;
}
