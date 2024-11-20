using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL;

public class Query
{
    private readonly ILogger<Query> _logger;

    public Query(ILogger<Query> logger)
    {
        _logger = logger;
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<Patient>> GetPatients([Service] PatientDbContext context)
    {
        try
        {
            return await context.Patients
                .Include(p => p.DiagnosticResults)
                .Include(p => p.MriImages)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients");
            throw;
        }
    }

    public async Task<Patient?> GetPatient([Service] PatientDbContext context, int id)
    {
        try
        {
            return await context.Patients
                .Include(p => p.DiagnosticResults)
                .Include(p => p.MriImages)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient with ID {PatientId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiagnosticResult>> GetDiagnosticResults([Service] PatientDbContext context)
    {
        try
        {
            return await context.DiagnosticResults
                .Include(d => d.Patient)
                .OrderByDescending(d => d.Date)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching diagnostic results");
            throw;
        }
    }

    public async Task<IEnumerable<MriImage>> GetMriImages([Service] PatientDbContext context)
    {
        try
        {
            return await context.MriImages
                .Include(m => m.Patient)
                .OrderByDescending(m => m.AcquisitionDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching MRI images");
            throw;
        }
    }
}
