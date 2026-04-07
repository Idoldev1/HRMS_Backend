using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<Attendance>> GetByDateRangeAsync(int? employeeId, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<Attendance>> GetOpenAttendancesBeforeDateAsync(DateTime cutoffDate, int? employeeId = null);
    }
}