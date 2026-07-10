using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IEmployeeWorkLocationRepository : IRepository<EmployeeWorkLocation>
    {
        Task<EmployeeWorkLocation?> GetAssignmentAsync(int employeeId, int workLocationId);
        Task<IEnumerable<EmployeeWorkLocation>> GetByEmployeeIdAsync(int employeeId);
    }
}
