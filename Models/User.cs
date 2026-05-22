using System;
using System.Collections.Generic;

namespace EXE201_Backend.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public string Role { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool Verified { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool IsActive { get; set; }

    public string? GoogleUserId { get; set; }

    public string? AvatarLink { get; set; }

    public virtual ICollection<WorkshopParticipant> WorkshopParticipants { get; set; } = new List<WorkshopParticipant>();

    public virtual ICollection<WorkshopReview> WorkshopReviews { get; set; } = new List<WorkshopReview>();

    public virtual ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();

    public virtual ICollection<Workshop> WorkshopsNavigation { get; set; } = new List<Workshop>();
}
