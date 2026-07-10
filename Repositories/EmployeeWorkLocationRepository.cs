using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class EmployeeWorkLocationRepository : Repository<EmployeeWorkLocation>, IEmployeeWorkLocationRepository
    {
        public EmployeeWorkLocationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<EmployeeWorkLocation?> GetAssignmentAsync(int employeeId, int workLocationId)
        {
            return await _context.EmployeeWorkLocations
                .Include(ewl => ewl.WorkLocation)
                .FirstOrDefaultAsync(ewl => ewl.EmployeeId == employeeId && ewl.WorkLocationId == workLocationId);
        }

        public async Task<IEnumerable<EmployeeWorkLocation>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeWorkLocations
                .Include(ewl => ewl.WorkLocation)
                .Include(ewl => ewl.Employee)
                .ThenInclude(e => e!.Department)
                .Where(ewl => ewl.EmployeeId == employeeId)
                .ToListAsync();
        }
    }
}
