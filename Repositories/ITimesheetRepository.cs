using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface ITimesheetRepository : IRepository<Timesheet>
    {
        Task<Timesheet?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year);
        Task<IEnumerable<Timesheet>> GetTimesheetsAsync(int? employeeId, int? month, int? year, string? status);
        Task<IEnumerable<Timesheet>> GetPendingApprovalsAsync();
        Task<Timesheet?> GetWithDetailsAsync(int id);
        Task AddAuditLogAsync(TimesheetAuditLog log);
        Task UpdateEntryAsync(TimesheetEntry entry);
    }
}
