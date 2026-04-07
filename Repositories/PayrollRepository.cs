using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class PayrollRepository : Repository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee!).ThenInclude(e => e.Department)
                .Where(p => p.EmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payroll>> GetByStatusAsync(string status)
        {
            return await _context.Payrolls
                .Include(p => p.Employee!).ThenInclude(e => e.Department)
                .Where(p => p.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payrolls
                .Include(p => p.Employee!).ThenInclude(e => e.Department)
                .Where(p => p.PayPeriodStartDate >= startDate && p.PayPeriodEndDate <= endDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsWithIncludesAsync(int? employeeId, string? status)
        {
            var query = _context.Payrolls
                .Include(p => p.Employee!).ThenInclude(e => e.Department)
                .AsQueryable();

            if (employeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task<Payroll?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee!).ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}