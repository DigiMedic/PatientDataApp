using HotChocolate;
using HotChocolate.Types;
using PatientDataApp.Data;
using PatientDataApp.Models;
using Microsoft.EntityFrameworkCore;

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
        return await context.Patients.ToListAsync();
    }

    public async Task<Patient?> GetPatientById(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Patients.FirstOrDefaultAsync(p => p.Id == id);
    }
}
