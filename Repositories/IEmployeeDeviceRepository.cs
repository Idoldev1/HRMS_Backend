using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IEmployeeDeviceRepository : IRepository<EmployeeDevice>
    {
        Task<IEnumerable<EmployeeDevice>> GetByEmployeeIdAsync(int employeeId);
        Task<EmployeeDevice?> GetByDeviceIdAndEmployeeAsync(string deviceId, int employeeId);
        Task<bool> DeviceIdExistsForEmployeeAsync(string deviceId, int employeeId);
    }
}
