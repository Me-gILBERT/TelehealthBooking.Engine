using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TelehealthBooking.Domain.Entities;

namespace TelehealthBooking.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    // The constructor accepts options (like the connection string) from the API layer
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // These DbSets represent your SQL tables
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This tells EF Core to look for our configuration files (Step 2) automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
