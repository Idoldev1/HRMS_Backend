using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetByEmailAsync(string email);
    }
}
