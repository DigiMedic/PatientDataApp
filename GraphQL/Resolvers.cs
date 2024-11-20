using PatientDataApp.Data;
using PatientDataApp.Models;
using Microsoft.EntityFrameworkCore;

namespace PatientDataApp.GraphQL;

public class Resolvers
{
    private readonly PatientDbContext _context;
    private readonly ILogger<Resolvers> _logger;

    public Resolvers(PatientDbContext context, ILogger<Resolvers> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Patient?> GetPatient(MriImage mriImage, PatientDbContext context)
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

    public async Task<Patient?> GetPatient(DiagnosticResult diagnosticResult, PatientDbContext context)
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

    public async Task<IEnumerable<DiagnosticResult>> GetDiagnosticResults(Patient patient, PatientDbContext context)
    {
        try
        {
            return await context.DiagnosticResults
                .Where(d => d.PatientId == patient.Id)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving diagnostic results for patient {PatientId}", patient.Id);
            throw;
        }
    }
}
