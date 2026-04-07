using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IPayrollRepository : IRepository<Payroll>
    {
        Task<IEnumerable<Payroll>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Payroll>> GetByStatusAsync(string status);
        Task<IEnumerable<Payroll>> GetByPeriodAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Payroll>> GetPayrollsWithIncludesAsync(int? employeeId, string? status);
        Task<Payroll?> GetByIdWithIncludesAsync(int id);
    }
}