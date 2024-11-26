using PatientDataApp.Data;
using PatientDataApp.Models;
using Microsoft.EntityFrameworkCore;
using HotChocolate;

namespace PatientDataApp.GraphQL;

public class Resolvers
{
    private readonly IDbContextFactory<PatientDbContext> _contextFactory;
    private readonly ILogger<Resolvers> _logger;

    public Resolvers(IDbContextFactory<PatientDbContext> contextFactory, ILogger<Resolvers> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<DiagnosticResult>> GetDiagnosticResults(Patient patient, [Service(ServiceKind.Pooled)] PatientDbContext context)
    {
        try
        {
            return await context.DiagnosticResults
                .Where(d => d.PatientId == patient.Id)
                .OrderByDescending(d => d.Date)
                .AsSplitQuery()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving diagnostic results for patient {PatientId}", patient.Id);
            throw;
        }
    }

    public async Task<IEnumerable<MriImage>> GetMriImages(Patient patient, [Service(ServiceKind.Pooled)] PatientDbContext context)
    {
        try
        {
            return await context.MriImages
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.AcquisitionDate)
                .AsSplitQuery()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving MRI images for patient {PatientId}", patient.Id);
            throw;
        }
    }

    public async Task<Patient?> GetPatientForDiagnosticResult(DiagnosticResult diagnosticResult, [Service(ServiceKind.Pooled)] PatientDbContext context)
    {
        try
        {
            return await context.Patients
                .FirstOrDefaultAsync(p => p.Id == diagnosticResult.PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving patient for diagnostic result {DiagnosticResultId}", diagnosticResult.Id);
            throw;
        }
    }

    public async Task<Patient?> GetPatientForMriImage(MriImage mriImage, [Service(ServiceKind.Pooled)] PatientDbContext context)
    {
        try
        {
            return await context.Patients
                .FirstOrDefaultAsync(p => p.Id == mriImage.PatientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving patient for MRI image {MriImageId}", mriImage.Id);
            throw;
        }
    }
}
