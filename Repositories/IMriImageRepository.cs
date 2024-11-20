using PatientDataApp.Models;

namespace PatientDataApp.Repositories
{
    public interface IMriImageRepository
    {
        Task<IEnumerable<MriImage>> GetAllByPatientIdAsync(int patientId);
        Task<MriImage> GetByIdAsync(int id);
        Task<MriImage> AddAsync(MriImage mriImage);
        Task DeleteAsync(int id);
        Task<IEnumerable<MriImage>> GetByDateRangeAsync(int patientId, DateTime startDate, DateTime endDate);
    }
} 