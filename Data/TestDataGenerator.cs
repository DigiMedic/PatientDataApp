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

            var faker = new Faker("en_US");
            var patients = new List<Patient>();

            // Generování pacientů
            for (int i = 0; i < patientCount; i++)
            {
                var gender = faker.PickRandom<Bogus.DataSets.Name.Gender>();
                var birthDate = faker.Date.Between(DateTime.UtcNow.AddYears(-90), DateTime.UtcNow.AddYears(-18));
                var lastExamDate = faker.Date.Recent().ToUniversalTime();
                
                var patient = new Patient
                {
                    FirstName = gender == Bogus.DataSets.Name.Gender.Female ? 
                        faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female) : 
                        faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
                    LastName = faker.Name.LastName(),
                    DateOfBirth = birthDate.ToUniversalTime(),
                    PersonalId = await GeneratePersonalId(birthDate, gender == Bogus.DataSets.Name.Gender.Female),
                    Gender = gender == Bogus.DataSets.Name.Gender.Female ? "F" : "M",
                    InsuranceCompany = faker.PickRandom(_insuranceCompanies),
                    LastDiagnosis = faker.PickRandom(_commonDiagnoses),
                    LastExaminationDate = lastExamDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Pacient {patient.FirstName} {patient.LastName} úspěšně uložen");

                // Generování diagnostických výsledků pro tohoto pacienta
                var diagnosticResultCount = faker.Random.Int(1, maxDiagnosticResultsPerPatient);
                for (int j = 0; j < diagnosticResultCount; j++)
                {
                    var date = faker.Date.Between(patient.CreatedAt, DateTime.UtcNow).ToUniversalTime();
                    var diagnosis = faker.PickRandom(_commonDiagnoses);

                    var result = new DiagnosticResult
                    {
                        PatientId = patient.Id,
                        Diagnosis = diagnosis,
                        Description = faker.Lorem.Paragraph(),
                        Date = date,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.DiagnosticResults.AddAsync(result);
                    
                    if (j == diagnosticResultCount - 1)
                    {
                        patient.LastDiagnosis = diagnosis;
                        patient.LastExaminationDate = date;
                        _context.Patients.Update(patient);
                    }
                }
                await _context.SaveChangesAsync();

                // Generování MRI snímků
                var mriCount = faker.Random.Int(0, maxMriImagesPerPatient);
                var mriImages = new List<MriImage>();

                for (int k = 0; k < mriCount; k++)
                {
                    var acquisitionDate = faker.Date.Between(patient.CreatedAt, DateTime.UtcNow).ToUniversalTime();
                    
                    var mriImage = new MriImage
                    {
                        PatientId = patient.Id,
                        ImagePath = $"/data/mri-images/patient_{patient.Id}_mri_{k + 1}.dcm",
                        AcquisitionDate = acquisitionDate,
                        Description = faker.Lorem.Sentence(),
                        Findings = faker.Lorem.Paragraph(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    mriImages.Add(mriImage);
                }

                await _context.MriImages.AddRangeAsync(mriImages);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"Úspěšně vygenerováno {patientCount} pacientů s jejich záznamy.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chyba při generování testovacích dat");
            throw;
        }
    }

    private async Task<string> GeneratePersonalId(DateTime birthDate, bool isFemale)
    {
        var year = birthDate.Year % 100;
        var month = birthDate.Month + (isFemale ? 50 : 0);
        var day = birthDate.Day;
        
        string personalId;
        do
        {
            var random = new Random().Next(1000);
            personalId = $"{year:00}{month:00}{day:00}/{random:000}";
        } while (await _context.Patients.AnyAsync(p => p.PersonalId == personalId));

        return personalId;
    }
}
