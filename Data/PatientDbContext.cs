using Microsoft.EntityFrameworkCore;
using PatientDataApp.Models;

namespace PatientDataApp.Data;

public class PatientDbContext : DbContext
{
    public PatientDbContext(DbContextOptions<PatientDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<DiagnosticResult> DiagnosticResults { get; set; }
    public DbSet<MriImage> MriImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurace pro Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PersonalId).IsRequired().HasMaxLength(20);
            
            // Indexy pro rychlejší vyhledávání
            entity.HasIndex(e => new { e.LastName, e.FirstName });
            entity.HasIndex(e => e.PersonalId).IsUnique();
        });

        // Konfigurace pro DiagnosticResult
        modelBuilder.Entity<DiagnosticResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Diagnosis).IsRequired();
            
            // Vztah s pacientem
            entity.HasOne(d => d.Patient)
                  .WithMany(p => p.DiagnosticResults)
                  .HasForeignKey(d => d.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index pro rychlejší vyhledávání
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.Date);
        });

        // Konfigurace pro MriImage
        modelBuilder.Entity<MriImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(255);
            
            // Vztah s pacientem
            entity.HasOne(m => m.Patient)
                  .WithMany(p => p.MriImages)
                  .HasForeignKey(m => m.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index pro rychlejší vyhledávání
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.AcquisitionDate);
        });
    }
}
