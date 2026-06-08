using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IJobPostingRepository : IRepository<JobPosting>
    {
        Task<IEnumerable<JobPosting>> GetAllWithApplicationCountAsync();
        Task<JobPosting?> GetByIdWithApplicationsAsync(int id);
        Task<JobPosting?> GetByPublicIdWithApplicationsAsync(string jobPostingId);
        Task<JobPosting?> GetByPublicIdAsync(string jobPostingId);
        Task<IEnumerable<JobPosting>> GetByStatusAsync(string status);
    }
}
