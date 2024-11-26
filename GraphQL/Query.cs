using HotChocolate;
using HotChocolate.Data;
using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL;

public class Query
{
    private readonly ILogger<Query> _logger;
    private readonly IDbContextFactory<PatientDbContext> _contextFactory;

    public Query(ILogger<Query> logger, IDbContextFactory<PatientDbContext> contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<Patient>> GetPatients()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            _logger.LogInformation("Fetching patients from database");
            
            var patients = await context.Patients
                .Include(p => p.DiagnosticResults)
                .Include(p => p.MriImages)
                .Take(100)  // Limit počtu záznamů pro testování
                .ToListAsync();
                
            _logger.LogInformation("Successfully fetched {Count} patients", patients.Count);
            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patients");
            throw;
        }
    }

    public async Task<Patient?> GetPatient(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Patients
                .Include(p => p.DiagnosticResults)
                .Include(p => p.MriImages)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching patient with ID {PatientId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DiagnosticResult>> GetDiagnosticResults()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
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

    public async Task<IEnumerable<MriImage>> GetMriImages()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
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
