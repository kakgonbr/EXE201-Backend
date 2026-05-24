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

    public virtual DbSet<HostRegistration> HostRegistrations { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workshop> Workshops { get; set; }

    public virtual DbSet<WorkshopCategory> WorkshopCategories { get; set; }

    public virtual DbSet<WorkshopImage> WorkshopImages { get; set; }

    public virtual DbSet<WorkshopLevel> WorkshopLevels { get; set; }

    public virtual DbSet<WorkshopParticipant> WorkshopParticipants { get; set; }

    public virtual DbSet<WorkshopReview> WorkshopReviews { get; set; }

    public virtual DbSet<WorkshopSchedule> WorkshopSchedules { get; set; }

    public virtual DbSet<WorkshopScheduleConfig> WorkshopScheduleConfigs { get; set; }

    public virtual DbSet<WorkshopTicket> WorkshopTickets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DATABASE_CONNECTION"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HostRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HostRegi__3214EC07CF3AB225");

            entity.HasIndex(e => e.UserId, "UQ__HostRegi__1788CC4D06411C96").IsUnique();

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.HostRegistrationApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("fk_hostregistration_user_approvedby");

            entity.HasOne(d => d.User).WithOne(p => p.HostRegistrationUser)
                .HasForeignKey<HostRegistration>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hostregistration_user");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => new { e.ParticipantId, e.TicketId }).HasName("pk_payment");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("failed");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07B268C2F6");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534E6A62744").IsUnique();

            entity.Property(e => e.AvatarLink)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GoogleUserId)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Location)
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
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC070D562CA9");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Language)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasDefaultValue("en");
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

            entity.HasOne(d => d.Level).WithMany(p => p.Workshops)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshop_level");
        });

        modelBuilder.Entity<WorkshopCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC078D2354E2");

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<WorkshopImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0792580A10");

            entity.Property(e => e.ImgLink)
                .HasMaxLength(256)
                .IsUnicode(false);

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopImages)
                .HasForeignKey(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopimage_workshop");
        });

        modelBuilder.Entity<WorkshopLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC076BD81825");

            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .IsUnicode(false);
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
                .HasDefaultValue("paid");

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
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC07034A08A3");

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
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC07C3ACD4CD");

            entity.HasOne(d => d.Workshop).WithMany(p => p.WorkshopSchedules)
                .HasForeignKey(d => d.WorkshopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workshopschedule_workshop");
        });

        modelBuilder.Entity<WorkshopScheduleConfig>(entity =>
        {
            entity.HasKey(e => e.WorkshopId).HasName("PK__Workshop__7A008C0A526C7E49");

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
            entity.HasKey(e => e.Id).HasName("PK__Workshop__3214EC0752803EFF");

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
