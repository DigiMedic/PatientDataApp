using HotChocolate;
using HotChocolate.Types;
using PatientDataApp.Data;
using PatientDataApp.Models;
using Microsoft.EntityFrameworkCore;
using PatientDataApp.Repositories;

namespace PatientDataApp.GraphQL;

[GraphQLDescription("Queries for retrieving patients")]
public class Query
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public Query(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<Patient>> GetPatients()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Patients
            .Select(p => new Patient
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                LastDiagnosis = p.LastDiagnosis
            })
            .ToListAsync();
    }

    public async Task<Patient?> GetPatientById(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Patients.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<MriImage>> GetMriImages(
        [Service] IMriImageRepository repository,
        int patientId)
    {
        return await repository.GetAllByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<MriImage>> GetMriImagesByDateRange(
        [Service] IMriImageRepository repository,
        int patientId,
        DateTime startDate,
        DateTime endDate)
    {
        return await repository.GetByDateRangeAsync(patientId, startDate, endDate);
    }

    [UseFiltering]
    [UseSorting]
    public async Task<IEnumerable<MriImage>> SearchMriImages(
        [Service] IMriImageRepository repository,
        MriImageFilterInput filter)
    {
        var query = repository.GetQueryable();

        if (filter.PatientId.HasValue)
            query = query.Where(m => m.PatientId == filter.PatientId);

        if (!string.IsNullOrEmpty(filter.StudyType))
            query = query.Where(m => m.StudyType == filter.StudyType);

        if (!string.IsNullOrEmpty(filter.BodyPart))
            query = query.Where(m => m.BodyPart == filter.BodyPart);

        if (filter.StartDate.HasValue)
            query = query.Where(m => m.UploadedAt >= filter.StartDate);

        if (filter.EndDate.HasValue)
            query = query.Where(m => m.UploadedAt <= filter.EndDate);

        if (filter.Tags != null && filter.Tags.Any())
            query = query.Where(m => filter.Tags.All(t => m.Tags.ContainsKey(t)));

        return await query
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    [GraphQLDescription("Získá statistiky MRI snímků")]
    public async Task<MriStatistics> GetMriStatistics(
        [Service] IMriImageRepository repository,
        int? patientId = null)
    {
        var query = repository.GetQueryable();
        if (patientId.HasValue)
            query = query.Where(m => m.PatientId == patientId);

        return new MriStatistics
        {
            TotalCount = await query.CountAsync(),
            StudyTypeCounts = await query
                .GroupBy(m => m.StudyType)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToListAsync(),
            BodyPartCounts = await query
                .GroupBy(m => m.BodyPart)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToListAsync()
        };
    }
}

public class MriStatistics
{
    public int TotalCount { get; set; }
    public List<KeyValuePair<string, int>> StudyTypeCounts { get; set; }
    public List<KeyValuePair<string, int>> BodyPartCounts { get; set; }
}
