using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class JobPostingRepository : Repository<JobPosting>, IJobPostingRepository
    {
        public JobPostingRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<JobPosting>> GetAllWithApplicationCountAsync()
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JobPosting?> GetByIdWithApplicationsAsync(int id)
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<JobPosting>> GetByStatusAsync(string status)
        {
            return await _context.JobPostings
                .Include(j => j.Applications)
                .Where(j => j.Status == status)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }
    }
}
