using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IPerformanceRepository : IRepository<PerformanceReview>
    {
        Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PerformanceReview>> GetReviewsWithIncludesAsync(int? employeeId);
    }
}