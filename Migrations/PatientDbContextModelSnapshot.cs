﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PatientDataApp.Data;

#nullable disable

namespace PatientDataApp.Migrations
{
    [DbContext(typeof(PatientDbContext))]
    partial class PatientDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PatientDataApp.Models.DiagnosticResult", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Diagnosis")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("diagnosis");

                    b.Property<int>("PatientId")
                        .HasColumnType("integer")
                        .HasColumnName("patient_id");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_diagnostic_results");

                    b.HasIndex("Date")
                        .HasDatabaseName("ix_diagnostic_results_date");

                    b.HasIndex("PatientId")
                        .HasDatabaseName("ix_diagnostic_results_patient_id");

                    b.ToTable("diagnostic_results", (string)null);
                });

            modelBuilder.Entity("PatientDataApp.Models.MriImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AcquisitionDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("acquisition_date");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)")
                        .HasColumnName("description");

                    b.Property<string>("Findings")
                        .HasColumnType("text")
                        .HasColumnName("findings");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("image_path");

                    b.Property<int>("PatientId")
                        .HasColumnType("integer")
                        .HasColumnName("patient_id");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_mri_images");

                    b.HasIndex("AcquisitionDate")
                        .HasDatabaseName("ix_mri_images_acquisition_date");

                    b.HasIndex("PatientId")
                        .HasDatabaseName("ix_mri_images_patient_id");

                    b.ToTable("mri_images", (string)null);
                });

            modelBuilder.Entity("PatientDataApp.Models.Patient", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_of_birth");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("first_name");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("character varying(1)")
                        .HasColumnName("gender");

                    b.Property<string>("InsuranceCompany")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("insurance_company");

                    b.Property<string>("LastDiagnosis")
                        .HasColumnType("text")
                        .HasColumnName("last_diagnosis");

                    b.Property<DateTime?>("LastExaminationDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_examination_date");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("last_name");

                    b.Property<string>("PersonalId")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("personal_id");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("phone_number");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("updated_at");

                    b.HasKey("Id")
                        .HasName("pk_patients");

                    b.HasIndex("PersonalId")
                        .IsUnique()
                        .HasDatabaseName("ix_patients_personal_id");

                    b.HasIndex("LastName", "FirstName")
                        .HasDatabaseName("ix_patients_last_name_first_name");

                    b.ToTable("patients", (string)null);
                });

            modelBuilder.Entity("PatientDataApp.Models.DiagnosticResult", b =>
                {
                    b.HasOne("PatientDataApp.Models.Patient", "Patient")
                        .WithMany("DiagnosticResults")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_diagnostic_results_patients_patient_id");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("PatientDataApp.Models.MriImage", b =>
                {
                    b.HasOne("PatientDataApp.Models.Patient", "Patient")
                        .WithMany("MriImages")
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_mri_images_patients_patient_id");

                    b.Navigation("Patient");
                });

            modelBuilder.Entity("PatientDataApp.Models.Patient", b =>
                {
                    b.Navigation("DiagnosticResults");

                    b.Navigation("MriImages");
                });
#pragma warning restore 612, 618
        }
    }
}
