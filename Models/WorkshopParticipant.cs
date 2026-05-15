using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopParticipant
{
    public int ParticipantId { get; set; }

    public int TicketId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime BookedOn { get; set; }

    public virtual User Participant { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual WorkshopTicket Ticket { get; set; } = null!;
}
