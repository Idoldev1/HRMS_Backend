using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class WorkLocationRepository : Repository<WorkLocation>, IWorkLocationRepository
    {
        public WorkLocationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<WorkLocation>> GetActiveLocationsAsync()
        {
            return await _context.WorkLocations
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkLocation>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeWorkLocations
                .Include(ewl => ewl.WorkLocation)
                .Where(ewl => ewl.EmployeeId == employeeId && ewl.IsActive && ewl.WorkLocation!.IsActive)
                .Select(ewl => ewl.WorkLocation!)
                .ToListAsync();
        }
    }
}
