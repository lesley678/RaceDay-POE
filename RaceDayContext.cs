
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayContext : DbContext
{
    public RaceDayContext(DbContextOptions<RaceDayContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Enrolment>()
    .HasOne(e => e.Participant)
    .WithMany()
    .HasForeignKey(e => e.PARTICIPANT_ID)
    .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CATEGORY_ID)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EMAIL)
            .IsUnique();

        modelBuilder.Entity<Participant>()
            .HasIndex(p => p.USER_ID)
            .IsUnique();

        modelBuilder.Entity<Organiser>()
            .HasIndex(o => o.USER_ID)
            .IsUnique();

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.PARTICIPANT_ID, e.CATEGORY_ID })
            .IsUnique();

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.ENROLMENT_ID)
            .IsUnique();

        modelBuilder.Entity<User>()
            .ToTable("USER", t =>
                t.HasCheckConstraint(
                    "CK_USER_ROLE",
                    "[ROLE] IN ('Participant', 'Organiser')"));

        modelBuilder.Entity<Category>()
            .ToTable("CATEGORY", t =>
                t.HasCheckConstraint(
                    "CK_CATEGORY_DISTANCE", "[DISTANCE] > 0"));

        modelBuilder.Entity<Category>()
            .ToTable("CATEGORY", t =>
                t.HasCheckConstraint(
                    "CK_CATEGORY_ENTRY_FEE", "[ENTRY_FEE] >= 0"));

        modelBuilder.Entity<Enrolment>()
     .ToTable("ENROLMENT", t =>
         t.HasCheckConstraint(
             "CK_ENROLMENT_STATUS",
             "[STATUS] IN ('Pending', 'Active', 'Cancelled', 'Completed')"));

        modelBuilder.Entity<Result>()
            .ToTable("RESULT", t =>
                t.HasCheckConstraint(
                    "CK_RESULT_POSITION", "[POSITION] IS NULL OR [POSITION] > 0"));

        modelBuilder.Entity<Result>()
            .ToTable("RESULT", t =>
                t.HasCheckConstraint(
                    "CK_RESULT_STATUS",
                    "[RESULT_STATUS] IN ('Pending', 'Finished', 'Did Not Finish', 'Disqualified')"));
    }
}

