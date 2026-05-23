using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class Workshop
{
    public int Id { get; set; }

    public string? ThumbnailLink { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Location { get; set; } = null!;

    public int CategoryId { get; set; }

    public int Duration { get; set; }

    public int LevelId { get; set; }

    public string Language { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public string Status { get; set; } = null!;

    public virtual WorkshopCategory Category { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual WorkshopLevel Level { get; set; } = null!;

    public virtual ICollection<WorkshopImage> WorkshopImages { get; set; } = new List<WorkshopImage>();

    public virtual ICollection<WorkshopReview> WorkshopReviews { get; set; } = new List<WorkshopReview>();

    public virtual WorkshopScheduleConfig? WorkshopScheduleConfig { get; set; }

    public virtual ICollection<WorkshopSchedule> WorkshopSchedules { get; set; } = new List<WorkshopSchedule>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
