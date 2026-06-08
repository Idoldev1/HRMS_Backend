using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class TimesheetRepository : Repository<Timesheet>, ITimesheetRepository
    {
        public TimesheetRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Timesheet?> GetByEmployeeMonthYearAsync(int employeeId, int month, int year)
        {
            return await _context.Timesheets
                .Include(t => t.Employee).ThenInclude(e => e!.Department)
                .Include(t => t.ApprovedBy)
                .Include(t => t.Entries)
                .FirstOrDefaultAsync(t => t.EmployeeId == employeeId && t.Month == month && t.Year == year);
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsAsync(int? employeeId, int? month, int? year, string? status)
        {
            var query = _context.Timesheets
                .Include(t => t.Employee).ThenInclude(e => e!.Department)
                .Include(t => t.ApprovedBy)
                .AsQueryable();

            if (employeeId.HasValue)
                query = query.Where(t => t.EmployeeId == employeeId.Value);

            if (month.HasValue)
                query = query.Where(t => t.Month == month.Value);

            if (year.HasValue)
                query = query.Where(t => t.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            return await query.OrderByDescending(t => t.Year).ThenByDescending(t => t.Month).ToListAsync();
        }

        public async Task<IEnumerable<Timesheet>> GetPendingApprovalsAsync()
        {
            return await _context.Timesheets
                .Include(t => t.Employee).ThenInclude(e => e!.Department)
                .Where(t => t.Status == "Submitted")
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Timesheet?> GetWithDetailsAsync(int id)
        {
            return await _context.Timesheets
                .Include(t => t.Employee).ThenInclude(e => e!.Department)
                .Include(t => t.ApprovedBy)
                .Include(t => t.Entries!.OrderBy(e => e.Date))
                .Include(t => t.AuditLogs!.OrderByDescending(a => a.PerformedAt))
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddAuditLogAsync(TimesheetAuditLog log)
        {
            await _context.TimesheetAuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEntryAsync(TimesheetEntry entry)
        {
            _context.TimesheetEntries.Update(entry);
            await _context.SaveChangesAsync();
        }
    }
}
