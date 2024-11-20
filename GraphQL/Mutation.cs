using HotChocolate;
using HotChocolate.Types;
using PatientDataApp.Data;
using PatientDataApp.Models;
using PatientDataApp.GraphQL;
using Microsoft.EntityFrameworkCore;

namespace PatientDataApp.GraphQL;

[GraphQLDescription("Mutations for managing patients")]
public class Mutation
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public Mutation(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [GraphQLDescription("Add a new patient")]
    public async Task<Patient> AddPatient(
        string name, 
        int age, 
        string lastDiagnosis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        try 
        {
            var patient = new Patient
            {
                Name = name,
                Age = age,
                LastDiagnosis = lastDiagnosis
            };

            await context.Patients.AddAsync(patient);
            await context.SaveChangesAsync();

            return patient;
        }
        catch (Exception ex)
        {
            throw new GraphQLException(
                new Error("Failed to add patient", ex.Message));
        }
    }

    [GraphQLDescription("Update existing patient")]
    public async Task<Patient> UpdatePatient(int id, string name, int age, string lastDiagnosis)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var patient = await context.Patients.FirstOrDefaultAsync(p => p.Id == id);

        if (patient == null)
        {
            throw new GraphQLException(new Error("Patient not found", "NOT_FOUND"));
        }

        patient.Name = name;
        patient.Age = age;
        patient.LastDiagnosis = lastDiagnosis;

        await context.SaveChangesAsync();

        return patient;
    }
}
