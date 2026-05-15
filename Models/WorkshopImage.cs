using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopImage
{
    public int Id { get; set; }

    public int WorkshopId { get; set; }

    public string ImgLink { get; set; } = null!;

    public virtual Workshop Workshop { get; set; } = null!;
}
