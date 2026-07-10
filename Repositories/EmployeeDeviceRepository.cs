using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class EmployeeDeviceRepository : Repository<EmployeeDevice>, IEmployeeDeviceRepository
    {
        public EmployeeDeviceRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<EmployeeDevice>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeDevices
                .Include(d => d.Employee)
                .ThenInclude(e => e!.Department)
                .Where(d => d.EmployeeId == employeeId)
                .OrderByDescending(d => d.RegisteredAt)
                .ToListAsync();
        }

        public async Task<EmployeeDevice?> GetByDeviceIdAndEmployeeAsync(string deviceId, int employeeId)
        {
            return await _context.EmployeeDevices
                .Include(d => d.Employee)
                .ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.EmployeeId == employeeId);
        }

        public async Task<bool> DeviceIdExistsForEmployeeAsync(string deviceId, int employeeId)
        {
            return await _context.EmployeeDevices
                .AnyAsync(d => d.DeviceId == deviceId && d.EmployeeId == employeeId);
        }
    }
}
