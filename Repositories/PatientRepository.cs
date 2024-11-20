using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly PatientDbContext _context;

    public PatientRepository(PatientDbContext context)
    {
        _context = context;
    }

    // Základní CRUD operace
    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients
            .Include(p => p.MriImages)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.MriImages)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient> AddAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task UpdateAsync(Patient patient)
    {
        patient.UpdatedAt = DateTime.UtcNow;
        _context.Entry(patient).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
        }
    }

    // Vyhledávání
    public async Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.FirstName.Contains(searchTerm) || 
                       p.LastName.Contains(searchTerm))
            .Include(p => p.MriImages)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<Patient?> GetByPersonalIdAsync(string personalId)
    {
        return await _context.Patients
            .Include(p => p.MriImages)
            .FirstOrDefaultAsync(p => p.PersonalId == personalId);
    }

    public async Task<IEnumerable<Patient>> GetByInsuranceCompanyAsync(string insuranceCompany)
    {
        return await _context.Patients
            .Where(p => p.InsuranceCompany == insuranceCompany)
            .Include(p => p.MriImages)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    // Filtrování
    public async Task<IEnumerable<Patient>> GetPatientsWithRecentExaminationAsync(DateTime since)
    {
        return await _context.Patients
            .Where(p => p.LastExaminationDate >= since)
            .Include(p => p.MriImages)
            .OrderByDescending(p => p.LastExaminationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge)
    {
        var today = DateTime.Today;
        var maxDate = today.AddYears(-minAge);
        var minDate = today.AddYears(-maxAge - 1);

        return await _context.Patients
            .Where(p => p.DateOfBirth <= maxDate && p.DateOfBirth > minDate)
            .Include(p => p.MriImages)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    // MRI související operace
    public async Task<IEnumerable<Patient>> GetPatientsWithMriAsync()
    {
        return await _context.Patients
            .Where(p => p.MriImages != null && p.MriImages.Any())
            .Include(p => p.MriImages)
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<int> GetMriCountAsync(int patientId)
    {
        return await _context.MriImages
            .CountAsync(m => m.PatientId == patientId);
    }

    // Validace
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(p => p.Id == id);
    }

    public async Task<bool> IsPersonalIdUniqueAsync(string personalId)
    {
        return !await _context.Patients.AnyAsync(p => p.PersonalId == personalId);
    }
}
