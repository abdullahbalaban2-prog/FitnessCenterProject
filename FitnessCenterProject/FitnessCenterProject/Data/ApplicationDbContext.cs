using System.Collections.Generic;
using System.Reflection.Emit;
using FitnessCenterProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterProject.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    public DbSet<FitnessCenter> FitnessCenters { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Trainer> Trainers { get; set; } = null!;
    public DbSet<TrainerService> TrainerServices { get; set; } = null!;
    public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; } = null!;
    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<AiRecommendation> AiRecommendations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TrainerService (çoktan çoğa) için bileşik primary key
        builder.Entity<TrainerService>()
            .HasKey(ts => new { ts.TrainerId, ts.ServiceId });
    }
}
