using Microsoft.EntityFrameworkCore;
using PatientDataApp.Models;
using Microsoft.Extensions.Configuration;

namespace PatientDataApp.Data;

public class PatientDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly bool _isDevelopment;

    public PatientDbContext(IConfiguration configuration, DbContextOptions<PatientDbContext> options)
        : base(options)
    {
        _configuration = configuration;
        _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<DiagnosticResult> DiagnosticResults { get; set; }
    public DbSet<MriImage> MriImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>().ToTable("patients");
        modelBuilder.Entity<DiagnosticResult>().ToTable("diagnostic_results");
        modelBuilder.Entity<MriImage>().ToTable("mri_images");

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PersonalId).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => new { e.LastName, e.FirstName });
            entity.HasIndex(e => e.PersonalId).IsUnique();
        });

        modelBuilder.Entity<DiagnosticResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Diagnosis).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
            
            entity.HasOne(d => d.Patient)
                  .WithMany(p => p.DiagnosticResults)
                  .HasForeignKey(d => d.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.Date);
        });

        modelBuilder.Entity<MriImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImagePath).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AcquisitionDate).IsRequired();
            
            entity.HasOne(m => m.Patient)
                  .WithMany(p => p.MriImages)
                  .HasForeignKey(m => m.PatientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.AcquisitionDate);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.EnableRetryOnFailure(3);
                options.CommandTimeout(30);
            })
            .UseSnakeCaseNamingConvention();

            if (_isDevelopment)
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }
    }
}
