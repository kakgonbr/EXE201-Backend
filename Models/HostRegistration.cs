using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class HostRegistration
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool Approved { get; set; }

    public int? ApprovedBy { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
