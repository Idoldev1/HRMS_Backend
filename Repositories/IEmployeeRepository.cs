using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetAllWithIncludesAsync();
        Task<Employee?> GetByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<Employee>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Employee>> GetByStatusAsync(string status);
        Task<IEnumerable<Employee>> SearchAsync(string searchTerm);
        Task<Employee?> GetByIdWithIncludesAsync(int id);
    }
}