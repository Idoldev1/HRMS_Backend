using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IJobApplicationRepository : IRepository<JobApplication>
    {
        Task<IEnumerable<JobApplication>> GetByJobPostingIdAsync(int jobPostingId);
        Task<JobApplication?> GetByJobAndEmailAsync(int jobPostingId, string email);
        Task<JobApplication?> GetByIdWithJobAsync(int id);
    }
}
