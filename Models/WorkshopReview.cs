using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class WorkshopReview
{
    public int Id { get; set; }

    public int WorkshopId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int Rating { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? Response { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Workshop Workshop { get; set; } = null!;
}
