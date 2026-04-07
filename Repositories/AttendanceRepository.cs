using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .Where(a => a.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByDateRangeAsync(int? employeeId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            if (startDate.HasValue)
            {
                var fromDate = startDate.Value.Date;
                query = query.Where(a => a.Date.Date >= fromDate);
            }

            if (endDate.HasValue)
            {
                var toDate = endDate.Value.Date;
                query = query.Where(a => a.Date.Date <= toDate);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetOpenAttendancesBeforeDateAsync(DateTime cutoffDate, int? employeeId = null)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
                .Where(a => !a.CheckOut.HasValue && a.Date < cutoffDate)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            return await query.ToListAsync();
        }
    }
}