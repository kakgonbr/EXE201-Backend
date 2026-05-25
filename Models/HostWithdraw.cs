using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class HostWithdraw
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string BankName { get; set; } = null!;

    public string BankAccount { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public int? UpdatedBy { get; set; }

    public string? Note { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;
}
