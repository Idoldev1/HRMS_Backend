using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IWorkLocationRepository : IRepository<WorkLocation>
    {
        Task<IEnumerable<WorkLocation>> GetActiveLocationsAsync();
        Task<IEnumerable<WorkLocation>> GetByEmployeeIdAsync(int employeeId);
    }
}
