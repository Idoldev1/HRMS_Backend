using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class PerformanceRepository : Repository<PerformanceReview>, IPerformanceRepository
    {
        public PerformanceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.PerformanceReviews
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceReview>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PerformanceReviews
                .Include(p => p.Employee)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PerformanceReview>> GetReviewsWithIncludesAsync(int? employeeId)
        {
            var query = _context.PerformanceReviews
                .Include(p => p.Employee)
                .Include(p => p.ReviewedBy)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == employeeId.Value);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }
    }
}