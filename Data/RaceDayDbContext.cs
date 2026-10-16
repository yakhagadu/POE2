using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<EventOrganiser> EventOrganisers => Set<EventOrganiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Organiser).WithOne(o => o.User)
            .HasForeignKey<EventOrganiser>(o => o.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Participant).WithOne(p => p.User)
            .HasForeignKey<Participant>(p => p.UserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventOrganiser>()
            .HasMany(o => o.Events).WithOne(e => e.Organiser)
            .HasForeignKey(e => e.OrganiserId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Categories).WithOne(c => c.Event)
            .HasForeignKey(c => c.EventId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Participant>()
            .HasMany(p => p.Enrolments).WithOne(e => e.Participant)
            .HasForeignKey(e => e.ParticipantId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>()
            .HasMany(c => c.Enrolments).WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Result).WithOne(r => r.Enrolment)
            .HasForeignKey<Result>(r => r.EnrolmentId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.ParticipantId, e.CategoryId }).IsUnique();

        modelBuilder.Entity<Event>().Property(e => e.EntryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasPrecision(10, 2);
        modelBuilder.Entity<Category>().Property(c => c.DistanceKm).HasPrecision(5, 2);
        modelBuilder.Entity<Result>().Property(r => r.Pace).HasPrecision(5, 2);
    }
}