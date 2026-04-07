using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Employee>> GetAllWithIncludesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .ToListAsync();
        }

        public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Department != null && e.Department.Name == department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> GetByStatusAsync(string status)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.Status == status)
                .ToListAsync();
        }

        public async Task<IEnumerable<Employee>> SearchAsync(string searchTerm)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.FirstName.Contains(searchTerm) ||
                           e.LastName.Contains(searchTerm) ||
                           e.Email.Contains(searchTerm) ||
                           e.EmployeeId.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Manager)
                .Include(e => e.Documents)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}