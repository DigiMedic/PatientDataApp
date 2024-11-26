using HotChocolate;
using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL;

public class Mutation
{
    public async Task<Patient> CreatePatient(
        string firstName, 
        string lastName, 
        DateTime dateOfBirth,
        string personalId,
        string gender,
        string? insuranceCompany,
        string? initialDiagnosis,
        [Service] PatientDbContext context)
    {
        // Kontrola unikátnosti PersonalId
        if (await context.Patients.AnyAsync(p => p.PersonalId == personalId))
        {
            throw new GraphQLException("Personal ID already exists.");
        }

        var patient = new Patient
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PersonalId = personalId,
            Gender = gender,
            InsuranceCompany = insuranceCompany,
            LastDiagnosis = initialDiagnosis,
            LastExaminationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        return patient;
    }

    public async Task<DiagnosticResult> AddDiagnosticResult(
        int patientId, 
        string diagnosis, 
        string? description,
        DateTime? examinationDate,
        [Service] PatientDbContext context)
    {
        var patient = await context.Patients.FindAsync(patientId);
        if (patient == null)
        {
            throw new GraphQLException("Patient not found.");
        }

        var diagnosticResult = new DiagnosticResult
        {
            PatientId = patientId,
            Diagnosis = diagnosis,
            Description = description,
            Date = examinationDate ?? DateTime.UtcNow
        };

        patient.LastDiagnosis = diagnosis;
        patient.LastExaminationDate = diagnosticResult.Date;
        patient.UpdatedAt = DateTime.UtcNow;

        context.DiagnosticResults.Add(diagnosticResult);
        await context.SaveChangesAsync();

        return diagnosticResult;
    }

    public async Task<MriImage> AddMriImage(
        int patientId,
        string imagePath,
        DateTime acquisitionDate,
        string? description,
        string? findings,
        [Service] PatientDbContext context)
    {
        var patient = await context.Patients.FindAsync(patientId);
        if (patient == null)
        {
            throw new GraphQLException("Patient not found.");
        }

        var mriImage = new MriImage
        {
            PatientId = patientId,
            ImagePath = imagePath,
            AcquisitionDate = acquisitionDate,
            Description = description,
            Findings = findings
        };

        patient.LastExaminationDate = acquisitionDate;
        patient.UpdatedAt = DateTime.UtcNow;

        context.MriImages.Add(mriImage);
        await context.SaveChangesAsync();

        return mriImage;
    }

    public async Task<Patient> UpdatePatientDiagnosticData(
        int patientId,
        string? diagnosis,
        string? insuranceCompany,
        [Service] PatientDbContext context)
    {
        var patient = await context.Patients.FindAsync(patientId);
        if (patient == null)
        {
            throw new GraphQLException("Patient not found.");
        }

        if (diagnosis != null)
        {
            patient.LastDiagnosis = diagnosis;
        }

        if (insuranceCompany != null)
        {
            patient.InsuranceCompany = insuranceCompany;
        }

        patient.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return patient;
    }

    public class GenerateTestDataResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public GeneratedCounts GeneratedCounts { get; set; } = new();
    }

    public class GeneratedCounts
    {
        public int Patients { get; set; }
        public int DiagnosticResults { get; set; }
        public int MriImages { get; set; }
    }

    public async Task<GenerateTestDataResult> GenerateTestData(
        [Service] TestDataGenerator generator,
        int patientCount = 10,
        int maxDiagnosticResultsPerPatient = 3,
        int maxMriImagesPerPatient = 2)
    {
        try
        {
            await generator.GenerateTestDataAsync(
                patientCount,
                maxDiagnosticResultsPerPatient,
                maxMriImagesPerPatient);

            return new GenerateTestDataResult
            {
                Success = true,
                Message = "Test data generated successfully",
                GeneratedCounts = new GeneratedCounts
                {
                    Patients = patientCount,
                    DiagnosticResults = patientCount * maxDiagnosticResultsPerPatient,
                    MriImages = patientCount * maxMriImagesPerPatient
                }
            };
        }
        catch (Exception ex)
        {
            return new GenerateTestDataResult
            {
                Success = false,
                Message = $"Error generating test data: {ex.Message}"
            };
        }
    }
}
