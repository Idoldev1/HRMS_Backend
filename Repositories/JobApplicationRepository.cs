using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class JobApplicationRepository : Repository<JobApplication>, IJobApplicationRepository
    {
        public JobApplicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<JobApplication>> GetByJobPostingIdAsync(int jobPostingId)
        {
            return await _context.JobApplications
                .Include(a => a.JobPosting)
                .Where(a => a.JobPostingId == jobPostingId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<JobApplication?> GetByJobAndEmailAsync(int jobPostingId, string email)
        {
            return await _context.JobApplications
                .FirstOrDefaultAsync(a => a.JobPostingId == jobPostingId && a.NormalizedCandidateEmail == email);
        }

        public async Task<JobApplication?> GetByIdWithJobAsync(int id)
        {
            return await _context.JobApplications
                .Include(a => a.JobPosting)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
