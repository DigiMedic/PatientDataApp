using Microsoft.EntityFrameworkCore;
using PatientDataApp.Models;

namespace PatientDataApp.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Patient>()
            .ToTable("Patients");
    }
}
