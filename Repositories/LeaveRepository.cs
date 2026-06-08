using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class LeaveRepository : Repository<Leave>, ILeaveRepository
    {
        public LeaveRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Leave>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetByStatusAsync(string status)
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetPendingLeavesAsync()
        {
            return await _context.Leaves
                .Include(l => l.Employee)
                .Where(l => l.Status == "Pending")
                .ToListAsync();
        }

        public async Task<IEnumerable<Leave>> GetLeavesWithIncludesAsync(int? employeeId, string? status)
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(l => l.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        public async Task<bool> HasActiveLeaveAsync(int employeeId, DateTime startDate, DateTime endDate)
        {
            var requestedStart = startDate.Date;
            var requestedEnd = endDate.Date;

            return await _context.Leaves.AnyAsync(l =>
                l.EmployeeId == employeeId &&
                l.Status == "Approved" &&
                l.StartDate.Date <= requestedEnd &&
                l.EndDate.Date >= requestedStart);
        }
    }
}