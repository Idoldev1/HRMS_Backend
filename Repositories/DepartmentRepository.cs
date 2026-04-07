using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetByNameAsync(string name)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == name);
        }
    }
}