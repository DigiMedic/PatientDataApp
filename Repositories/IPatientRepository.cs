using PatientDataApp.Models;

namespace PatientDataApp.Repositories;

public interface IPatientRepository
{
    // Základní CRUD operace
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient> AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
    
    // Vyhledávání
    Task<IEnumerable<Patient>> SearchByNameAsync(string searchTerm);
    Task<Patient?> GetByPersonalIdAsync(string personalId);
    Task<IEnumerable<Patient>> GetByInsuranceCompanyAsync(string insuranceCompany);
    
    // Filtrování
    Task<IEnumerable<Patient>> GetPatientsWithRecentExaminationAsync(DateTime since);
    Task<IEnumerable<Patient>> GetPatientsByAgeRangeAsync(int minAge, int maxAge);
    
    // MRI související operace
    Task<IEnumerable<Patient>> GetPatientsWithMriAsync();
    Task<int> GetMriCountAsync(int patientId);
    
    // Validace
    Task<bool> ExistsAsync(int id);
    Task<bool> IsPersonalIdUniqueAsync(string personalId);
}
