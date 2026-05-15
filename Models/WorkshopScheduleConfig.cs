using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopScheduleConfig
{
    public int WorkshopId { get; set; }

    public string RepeatType { get; set; } = null!;

    public string Repeats { get; set; } = null!;

    public virtual Workshop Workshop { get; set; } = null!;
}
