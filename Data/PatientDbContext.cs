using Microsoft.EntityFrameworkCore;
using PatientDataApp.Models;

namespace PatientDataApp.Data
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<MriImage> MriImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();
                
                entity.HasIndex(e => e.LastName);
                entity.HasIndex(e => new { e.LastName, e.FirstName });
            });

            modelBuilder.Entity<MriImage>(entity =>
            {
                entity.ToTable("MriImages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.DicomMetadata)
                      .HasColumnType("jsonb");

                entity.Property(e => e.Tags)
                      .HasColumnType("jsonb");

                entity.HasOne(m => m.Patient)
                      .WithMany()
                      .HasForeignKey(m => m.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.PatientId);
                entity.HasIndex(e => e.UploadedAt);
                entity.HasIndex(e => e.StudyType);
                entity.HasIndex(e => e.BodyPart);
                entity.HasIndex(e => e.IsPreview);
            });
        }
    }
} 