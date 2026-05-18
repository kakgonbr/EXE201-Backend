using System;
using System.Collections.Generic;
using EXE201_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE201_Backend.Data;

public partial class ExeContext : DbContext
{
    public ExeContext()
    {
    }

    public ExeContext(DbContextOptions<ExeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workshop> Workshops { get; set; }

    public virtual DbSet<WorkshopCategory> WorkshopCategories { get; set; }

    public virtual DbSet<WorkshopImage> WorkshopImages { get; set; }

    public virtual DbSet<WorkshopParticipant> WorkshopParticipants { get; set; }

    public virtual DbSet<WorkshopReview> WorkshopReviews { get; set; }

    public virtual DbSet<WorkshopSchedule> WorkshopSchedules { get; set; }

    public virtual DbSet<WorkshopScheduleConfig> WorkshopScheduleConfigs { get; set; }

    public virtual DbSet<WorkshopTicket> WorkshopTickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DATABASE_CONNECTION"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC0794C0E6BB");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("failed");

            entity.HasOne(d => d.WorkshopParticipant).WithMany(p => p.Payments)
                .HasForeignKey(d => new { d.ParticipantId, d.TicketId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_workshopparticipant");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0761DC1786");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105345CC8C482").IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GoogleUserId)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("user");

            entity.HasMany(d => d.WorkshopsNavigation).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "WorkshopLike",
                    r => r.HasOne<Workshop>().WithMany()
                        .HasForeignKey("WorkshopId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_workshoplike_workshop"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_workshoplike_user"),
                    j =>
                    {
                        j.HasKey("UserId", "WorkshopId").HasName("pk_workshoplike");
                        j.ToTable("WorkshopLikes");
                    });
        });

        modelBuilder.Entity<Workshop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC079E58A8A5");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InstructorImgLink)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.InstructorName).HasMaxLength(100);
            entity.Property(e => e.Language)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasDefaultValue("en");
            entity.Property(e => e.Level)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("elementary");
            entity.Property(e => e.Location).HasMaxLength(256);
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("draft");
            entity.Property(e => e.ThumbnailLink)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Category).WithMany(p => p.Workshops)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshop_category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Workshops)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshop_user");
        });

        modelBuilder.Entity<WorkshopCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC078E1CFF63");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<WorkshopImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC07C4AAB34D");

            entity.Property(e => e.ImgLink)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopImages)
                .HasForeignKey(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopimage_workshop");
        });

        modelBuilder.Entity<WorkshopParticipant>(entity =>
        {
            entity.HasKey(e => new { e.ParticipantId, e.TicketId }).HasName("pk_workshopparticipant");

            entity.Property(e => e.BookedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("unpaid");

            entity.HasOne(d => d.Participant).WithMany(p => p.WorkshopParticipants)
                .HasForeignKey(d => d.ParticipantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopparticipant_user");

            entity.HasOne(d => d.Ticket).WithMany(p => p.WorkshopParticipants)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopparticipant_workshopticket");
        });

        modelBuilder.Entity<WorkshopReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0784B86D21");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.WorkshopReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopreview_user");

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopReviews)
                .HasForeignKey(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopreview_workshop");
        });

        modelBuilder.Entity<WorkshopSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC07F84A271B");

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopSchedules)
                .HasForeignKey(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopschedule_workshop");
        });

        modelBuilder.Entity<WorkshopScheduleConfig>(entity =>
        {
            entity.HasKey(e => e.WorkshopId).HasName("PK__Workshop__7A008C0AE9DD5D31");

            entity.ToTable("WorkshopScheduleConfig");

            entity.Property(e => e.WorkshopId).ValueGeneratedNever();
            entity.Property(e => e.RepeatType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("week");
            entity.Property(e => e.Repeats)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.HasOne(d => d.Workshop).WithOne(p => p.WorkshopScheduleConfig)
                .HasForeignKey<WorkshopScheduleConfig>(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopscheduleconfig_workshop");
        });

        modelBuilder.Entity<WorkshopTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0710B81BA6");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TicketType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("morning");

            entity.HasOne(d => d.WorkshopSchedule).WithMany(p => p.WorkshopTickets)
                .HasForeignKey(d => d.WorkshopScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshoptickets_workshopschedule");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
