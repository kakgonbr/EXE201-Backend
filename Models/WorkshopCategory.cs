using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();
}
