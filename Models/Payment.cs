using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class Payment
{
    public int ParticipantId { get; set; }

    public int TicketId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
