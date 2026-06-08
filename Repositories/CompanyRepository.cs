using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Company?> GetByEmailAsync(string email)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == email);
        }
    }
}
