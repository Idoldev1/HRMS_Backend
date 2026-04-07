using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<Department?> GetByNameAsync(string name);
    }
}