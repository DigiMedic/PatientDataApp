using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.GraphQL
{
    public class Resolvers
    {
        public async Task<Patient> GetPatient(MriImage mriImage, 
            [ScopedService] PatientDbContext context)
        {
            return await context.Patients
                .FirstOrDefaultAsync(p => p.Id == mriImage.PatientId);
        }

        public async Task<IEnumerable<MriImage>> GetMriImages(Patient patient,
            [ScopedService] PatientDbContext context)
        {
            return await context.MriImages
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
        }
    }
} 