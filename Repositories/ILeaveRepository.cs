using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface ILeaveRepository : IRepository<Leave>
    {
        Task<IEnumerable<Leave>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Leave>> GetByStatusAsync(string status);
        Task<IEnumerable<Leave>> GetPendingLeavesAsync();
        Task<IEnumerable<Leave>> GetLeavesWithIncludesAsync(int? employeeId, string? status);
    }
}