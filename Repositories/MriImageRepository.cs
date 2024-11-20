using Microsoft.EntityFrameworkCore;
using PatientDataApp.Data;
using PatientDataApp.Models;

namespace PatientDataApp.Repositories
{
    public class MriImageRepository : IMriImageRepository
    {
        private readonly PatientDbContext _context;

        public MriImageRepository(PatientDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MriImage>> GetAllByPatientIdAsync(int patientId)
        {
            return await _context.MriImages
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
        }

        public async Task<MriImage> GetByIdAsync(int id)
        {
            return await _context.MriImages.FindAsync(id);
        }

        public async Task<MriImage> AddAsync(MriImage mriImage)
        {
            _context.MriImages.Add(mriImage);
            await _context.SaveChangesAsync();
            return mriImage;
        }

        public async Task DeleteAsync(int id)
        {
            var mriImage = await _context.MriImages.FindAsync(id);
            if (mriImage != null)
            {
                _context.MriImages.Remove(mriImage);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MriImage>> GetByDateRangeAsync(int patientId, DateTime startDate, DateTime endDate)
        {
            return await _context.MriImages
                .Where(m => m.PatientId == patientId && 
                           m.UploadedAt >= startDate && 
                           m.UploadedAt <= endDate)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
        }
    }
} 