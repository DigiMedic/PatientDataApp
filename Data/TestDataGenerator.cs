using Bogus;
using PatientDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace PatientDataApp.Data;

public class TestDataGenerator
{
    private readonly PatientDbContext _context;
    private readonly ILogger<TestDataGenerator> _logger;
    private readonly string[] _insuranceCompanies = new[]
    {
        "VZP", "ZPMV", "OZP", "VoZP", "ČPZP", "RBP"
    };

    private readonly string[] _commonDiagnoses = new[]
    {
        "Hypertenze", "Diabetes mellitus 2. typu", "Osteoartróza",
        "Astma bronchiale", "Ischemická choroba srdeční",
        "Vertebrogenní algický syndrom", "Depresivní porucha",
        "Hypothyreóza", "Obezita", "Dyslipidémie"
    };

    public TestDataGenerator(
        PatientDbContext context,
        ILogger<TestDataGenerator> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task GenerateTestDataAsync(
        int patientCount = 50,
        int maxDiagnosticResultsPerPatient = 5,
        int maxMriImagesPerPatient = 3)
    {
        try
        {
            _logger.LogInformation("Začínám generování testovacích dat...");

            var faker = new Faker("cs");
            var patients = new List<Patient>();

            // Generování pacientů
            for (int i = 0; i < patientCount; i++)
            {
                var gender = faker.PickRandom<Bogus.DataSets.Name.Gender>();
                var birthDate = faker.Date.Between(DateTime.Now.AddYears(-90), DateTime.Now.AddYears(-18));
                
                var patient = new Patient
                {
                    FirstName = gender == Bogus.DataSets.Name.Gender.Female ? 
                        faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female) : 
                        faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
                    LastName = faker.Name.LastName(),
                    DateOfBirth = birthDate,
                    PersonalId = GeneratePersonalId(birthDate, gender == Bogus.DataSets.Name.Gender.Female),
                    Gender = gender == Bogus.DataSets.Name.Gender.Female ? "F" : "M",
                    InsuranceCompany = faker.PickRandom(_insuranceCompanies),
                    LastDiagnosis = faker.PickRandom(_commonDiagnoses),
                    LastExaminationDate = faker.Date.Recent(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                patients.Add(patient);
            }

            await _context.Patients.AddRangeAsync(patients);
            await _context.SaveChangesAsync();

            // Generování diagnostických výsledků
            foreach (var patient in patients)
            {
                var diagnosticResultCount = faker.Random.Int(1, maxDiagnosticResultsPerPatient);
                var diagnosticResults = new List<DiagnosticResult>();

                for (int i = 0; i < diagnosticResultCount; i++)
                {
                    var date = faker.Date.Between(patient.CreatedAt, DateTime.UtcNow);
                    var diagnosis = faker.PickRandom(_commonDiagnoses);

                    var result = new DiagnosticResult
                    {
                        PatientId = patient.Id,
                        Diagnosis = diagnosis,
                        Description = faker.Lorem.Paragraph(),
                        Date = date
                    };

                    diagnosticResults.Add(result);

                    if (i == diagnosticResultCount - 1)
                    {
                        patient.LastDiagnosis = diagnosis;
                        patient.LastExaminationDate = date;
                    }
                }

                await _context.DiagnosticResults.AddRangeAsync(diagnosticResults);
            }

            await _context.SaveChangesAsync();

            // Generování MRI snímků
            foreach (var patient in patients)
            {
                var mriCount = faker.Random.Int(0, maxMriImagesPerPatient);
                var mriImages = new List<MriImage>();

                for (int i = 0; i < mriCount; i++)
                {
                    var acquisitionDate = faker.Date.Between(patient.CreatedAt, DateTime.UtcNow);
                    
                    var mriImage = new MriImage
                    {
                        PatientId = patient.Id,
                        ImagePath = $"/data/mri-images/patient_{patient.Id}_mri_{i + 1}.dcm",
                        AcquisitionDate = acquisitionDate,
                        Description = faker.Lorem.Sentence(),
                        Findings = faker.Lorem.Paragraph(),
                        CreatedAt = acquisitionDate
                    };

                    mriImages.Add(mriImage);
                }

                await _context.MriImages.AddRangeAsync(mriImages);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Úspěšně vygenerováno {patientCount} pacientů s jejich záznamy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při generování testovacích dat");
            throw;
        }
    }

    private string GeneratePersonalId(DateTime birthDate, bool isFemale)
    {
        var year = birthDate.Year % 100;
        var month = birthDate.Month + (isFemale ? 50 : 0);
        var day = birthDate.Day;
        var random = new Random().Next(1000);
        return $"{year:00}{month:00}{day:00}/{random:000}";
    }
}
