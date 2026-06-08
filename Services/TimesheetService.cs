using HRMS.API.Contracts.Timesheet;
using HRMS.API.Models;
using HRMS.API.Repositories;

namespace HRMS.API.Services
{
    public interface ITimesheetService
    {
        Task<(Timesheet? Result, string? Error)> GenerateTimesheetAsync(int employeeId, int month, int year, string performedByUserId, string performedByName);
        Task<Timesheet> SubmitTimesheetAsync(int timesheetId, int requestingEmployeeId, string performedByUserId, string performedByName);
        Task<Timesheet> UpdateEntriesAsync(int timesheetId, int requestingEmployeeId, List<UpdateTimesheetEntryRequest> entries);
        Task<Timesheet> ApproveTimesheetAsync(int timesheetId, int approvedByEmployeeId, string comment, string performedByUserId, string performedByName);
        Task<Timesheet> RejectTimesheetAsync(int timesheetId, int rejectedByEmployeeId, string reason, string performedByUserId, string performedByName);
        Task<IEnumerable<Timesheet>> GetTimesheetsAsync(int? employeeId, int? month, int? year, string? status);
        Task<Timesheet?> GetTimesheetByIdAsync(int id);
        Task<IEnumerable<Timesheet>> GetPendingApprovalsAsync();
        Task<byte[]> ExportTimesheetAsync(int timesheetId);
        Task<Timesheet> LockTimesheetAsync(int timesheetId, string lockedByUserId, string lockedByName);
    }

    public class TimesheetService : ITimesheetService
    {
        private readonly ITimesheetRepository _timesheetRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILeaveRepository _leaveRepository;
        private readonly ILogger<TimesheetService> _logger;

        public TimesheetService(
            ITimesheetRepository timesheetRepository,
            IAttendanceRepository attendanceRepository,
            ILeaveRepository leaveRepository,
            ILogger<TimesheetService> logger)
        {
            _timesheetRepository = timesheetRepository;
            _attendanceRepository = attendanceRepository;
            _leaveRepository = leaveRepository;
            _logger = logger;
        }

        public async Task<(Timesheet? Result, string? Error)> GenerateTimesheetAsync(int employeeId, int month, int year, string performedByUserId, string performedByName)
        {
            _logger.LogInformation("Generating timesheet for employee {EmployeeId}, {Month}/{Year}.", employeeId, month, year);

            var existing = await _timesheetRepository.GetByEmployeeMonthYearAsync(employeeId, month, year);
            if (existing != null)
            {
                if (existing.Status != "Draft")
                    return (null, $"A timesheet for {month}/{year} already exists with status '{existing.Status}' and cannot be regenerated.");

                // Allow re-generation of Draft timesheets by deleting and recreating
                await _timesheetRepository.DeleteAsync(existing);
            }

            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var attendanceRecords = (await _attendanceRepository.GetByDateRangeAsync(employeeId, firstDay, lastDay))
                .OrderBy(a => a.Date)
                .ToList();

            var approvedLeaves = (await _leaveRepository.GetLeavesWithIncludesAsync(employeeId, "Approved"))
                .Where(l => l.StartDate.Date <= lastDay && l.EndDate.Date >= firstDay)
                .ToList();

            var workingDays = CountWorkingDays(firstDay, lastDay);

            var presentDays = attendanceRecords.Count(a => a.Status == "Present" || a.Status == "Remote");
            var lateDays = attendanceRecords.Count(a => a.Status == "Late");
            var totalHours = attendanceRecords.Sum(a => a.TotalHours);

            // Build a set of leave-covered business dates
            var leaveDates = new HashSet<DateTime>();
            foreach (var leave in approvedLeaves)
            {
                var cursor = leave.StartDate.Date;
                while (cursor <= leave.EndDate.Date && cursor <= lastDay)
                {
                    if (cursor >= firstDay && cursor.DayOfWeek != DayOfWeek.Saturday && cursor.DayOfWeek != DayOfWeek.Sunday)
                        leaveDates.Add(cursor);
                    cursor = cursor.AddDays(1);
                }
            }

            // Exclude leave days already covered by an attendance record
            var attendanceDates = attendanceRecords.Select(a => a.Date.Date).ToHashSet();
            var onLeaveDays = leaveDates.Count(d => !attendanceDates.Contains(d));

            var workedOrLeaveDays = presentDays + lateDays + onLeaveDays;
            var absentDays = Math.Max(0, workingDays - workedOrLeaveDays);

            var entries = BuildEntries(firstDay, lastDay, attendanceRecords, leaveDates);

            var timesheet = new Timesheet
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                TotalWorkingDays = workingDays,
                PresentDays = presentDays,
                LateDays = lateDays,
                OnLeaveDays = onLeaveDays,
                AbsentDays = absentDays,
                TotalHours = totalHours,
                Status = "Draft",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Entries = entries,
            };

            var created = await _timesheetRepository.AddAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = created.Id,
                Action = "Generated",
                PerformedById = performedByUserId,
                PerformedByName = performedByName,
                PreviousStatus = null,
                NewStatus = "Draft",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Timesheet {TimesheetId} generated for employee {EmployeeId} ({Month}/{Year}).", created.Id, employeeId, month, year);
            return (created, null);
        }

        public async Task<Timesheet> SubmitTimesheetAsync(int timesheetId, int requestingEmployeeId, string performedByUserId, string performedByName)
        {
            _logger.LogInformation("Submitting timesheet {TimesheetId}.", timesheetId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            if (timesheet.EmployeeId != requestingEmployeeId)
                throw new UnauthorizedAccessException("You can only submit your own timesheets.");

            if (timesheet.IsLocked)
                throw new InvalidOperationException("This timesheet is locked and cannot be modified.");

            if (timesheet.Status != "Draft" && timesheet.Status != "Rejected")
                throw new InvalidOperationException($"Cannot submit a timesheet with status '{timesheet.Status}'. Only Draft or Rejected timesheets can be submitted.");

            var previous = timesheet.Status;
            timesheet.Status = "Submitted";
            timesheet.UpdatedAt = DateTime.Now;

            await _timesheetRepository.UpdateAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = timesheetId,
                Action = "Submitted",
                PerformedById = performedByUserId,
                PerformedByName = performedByName,
                PreviousStatus = previous,
                NewStatus = "Submitted",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Timesheet {TimesheetId} submitted by employee {EmployeeId}.", timesheetId, requestingEmployeeId);
            return timesheet;
        }

        public async Task<Timesheet> UpdateEntriesAsync(int timesheetId, int requestingEmployeeId, List<UpdateTimesheetEntryRequest> entries)
        {
            _logger.LogInformation("Updating entries for timesheet {TimesheetId}.", timesheetId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            if (timesheet.EmployeeId != requestingEmployeeId)
                throw new UnauthorizedAccessException("You can only update your own timesheets.");

            if (timesheet.IsLocked)
                throw new InvalidOperationException("This timesheet is locked and cannot be modified.");

            if (timesheet.Status != "Draft" && timesheet.Status != "Rejected")
                throw new InvalidOperationException($"Timesheet entries can only be edited when status is Draft or Rejected.");

            foreach (var req in entries)
            {
                var entry = timesheet.Entries?.FirstOrDefault(e => e.Id == req.EntryId);
                if (entry == null) continue;

                entry.TasksCompleted = req.TasksCompleted;
                entry.Notes = req.Notes;
                entry.UpdatedAt = DateTime.Now;
                await _timesheetRepository.UpdateEntryAsync(entry);
            }

            timesheet.UpdatedAt = DateTime.Now;
            await _timesheetRepository.UpdateAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = timesheetId,
                Action = "EntryUpdated",
                PerformedById = requestingEmployeeId.ToString(),
                PerformedByName = "Employee",
                Comment = $"Updated {entries.Count} entry/entries.",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Updated {Count} entries for timesheet {TimesheetId}.", entries.Count, timesheetId);
            return timesheet;
        }

        public async Task<Timesheet> ApproveTimesheetAsync(int timesheetId, int approvedByEmployeeId, string comment, string performedByUserId, string performedByName)
        {
            _logger.LogInformation("Approving timesheet {TimesheetId} by employee {ApprovedById}.", timesheetId, approvedByEmployeeId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            if (timesheet.IsLocked)
                throw new InvalidOperationException("This timesheet is locked.");

            if (timesheet.Status != "Submitted")
                throw new InvalidOperationException($"Only submitted timesheets can be approved. Current status: '{timesheet.Status}'.");

            var previous = timesheet.Status;
            timesheet.Status = "Approved";
            timesheet.ApprovedById = approvedByEmployeeId;
            timesheet.ApprovalComment = comment;
            timesheet.ApprovedAt = DateTime.Now;
            timesheet.UpdatedAt = DateTime.Now;

            await _timesheetRepository.UpdateAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = timesheetId,
                Action = "Approved",
                PerformedById = performedByUserId,
                PerformedByName = performedByName,
                Comment = comment,
                PreviousStatus = previous,
                NewStatus = "Approved",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Timesheet {TimesheetId} approved by {PerformedByName}.", timesheetId, performedByName);
            return timesheet;
        }

        public async Task<Timesheet> RejectTimesheetAsync(int timesheetId, int rejectedByEmployeeId, string reason, string performedByUserId, string performedByName)
        {
            _logger.LogInformation("Rejecting timesheet {TimesheetId}.", timesheetId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            if (timesheet.IsLocked)
                throw new InvalidOperationException("This timesheet is locked.");

            if (timesheet.Status != "Submitted")
                throw new InvalidOperationException($"Only submitted timesheets can be rejected. Current status: '{timesheet.Status}'.");

            var previous = timesheet.Status;
            timesheet.Status = "Rejected";
            timesheet.RejectionReason = reason;
            timesheet.UpdatedAt = DateTime.Now;

            await _timesheetRepository.UpdateAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = timesheetId,
                Action = "Rejected",
                PerformedById = performedByUserId,
                PerformedByName = performedByName,
                Comment = reason,
                PreviousStatus = previous,
                NewStatus = "Rejected",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Timesheet {TimesheetId} rejected. Reason: {Reason}.", timesheetId, reason);
            return timesheet;
        }

        public async Task<IEnumerable<Timesheet>> GetTimesheetsAsync(int? employeeId, int? month, int? year, string? status)
        {
            _logger.LogInformation("Retrieving timesheets. EmployeeId: {EmployeeId}, Month: {Month}, Year: {Year}, Status: {Status}.",
                employeeId, month, year, status);
            return await _timesheetRepository.GetTimesheetsAsync(employeeId, month, year, status);
        }

        public async Task<Timesheet?> GetTimesheetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving timesheet {TimesheetId}.", id);
            return await _timesheetRepository.GetWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Timesheet>> GetPendingApprovalsAsync()
        {
            _logger.LogInformation("Retrieving pending timesheet approvals.");
            return await _timesheetRepository.GetPendingApprovalsAsync();
        }

        public async Task<byte[]> ExportTimesheetAsync(int timesheetId)
        {
            _logger.LogInformation("Exporting timesheet {TimesheetId}.", timesheetId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            var html = BuildTimesheetHtml(timesheet);
            return System.Text.Encoding.UTF8.GetBytes(html);
        }

        public async Task<Timesheet> LockTimesheetAsync(int timesheetId, string lockedByUserId, string lockedByName)
        {
            _logger.LogInformation("Locking timesheet {TimesheetId}.", timesheetId);

            var timesheet = await _timesheetRepository.GetWithDetailsAsync(timesheetId)
                ?? throw new KeyNotFoundException($"Timesheet {timesheetId} not found.");

            if (timesheet.IsLocked)
                throw new InvalidOperationException("Timesheet is already locked.");

            if (timesheet.Status != "Approved")
                throw new InvalidOperationException("Only approved timesheets can be locked.");

            timesheet.IsLocked = true;
            timesheet.LockedAt = DateTime.Now;
            timesheet.LockedBy = lockedByName;
            timesheet.UpdatedAt = DateTime.Now;

            await _timesheetRepository.UpdateAsync(timesheet);

            await _timesheetRepository.AddAuditLogAsync(new TimesheetAuditLog
            {
                TimesheetId = timesheetId,
                Action = "Locked",
                PerformedById = lockedByUserId,
                PerformedByName = lockedByName,
                Comment = "Locked after payroll processing.",
                PreviousStatus = "Approved",
                NewStatus = "Approved",
                PerformedAt = DateTime.Now,
            });

            _logger.LogInformation("Timesheet {TimesheetId} locked by {LockedByName}.", timesheetId, lockedByName);
            return timesheet;
        }

        // ──────────────────────────── private helpers ────────────────────────────

        private static int CountWorkingDays(DateTime start, DateTime end)
        {
            int count = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    count++;
            }
            return count;
        }

        private static List<TimesheetEntry> BuildEntries(
            DateTime firstDay,
            DateTime lastDay,
            List<Attendance> attendanceRecords,
            HashSet<DateTime> leaveDates)
        {
            var entries = new List<TimesheetEntry>();
            var attendanceByDate = attendanceRecords.ToDictionary(a => a.Date.Date, a => a);

            for (var date = firstDay.Date; date <= lastDay.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                string? status = null;
                decimal hours = 0;

                if (attendanceByDate.TryGetValue(date, out var att))
                {
                    status = att.Status;
                    hours = att.TotalHours;
                }
                else if (leaveDates.Contains(date))
                {
                    status = "On Leave";
                }
                else
                {
                    status = "Absent";
                }

                entries.Add(new TimesheetEntry
                {
                    Date = date,
                    AttendanceStatus = status,
                    HoursWorked = hours,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }

            return entries;
        }

        private static string BuildTimesheetHtml(Timesheet t)
        {
            var emp = t.Employee;
            var empName = emp != null ? $"{emp.FirstName} {emp.LastName}" : $"Employee #{t.EmployeeId}";
            var empId = emp?.EmployeeId ?? t.EmployeeId.ToString();
            var dept = emp?.Department?.Name ?? "—";
            var pos = emp?.Position ?? "—";
            var period = new DateTime(t.Year, t.Month, 1).ToString("MMMM yyyy");
            var approvedByName = t.ApprovedBy != null ? $"{t.ApprovedBy.FirstName} {t.ApprovedBy.LastName}" : "—";

            var statusColor = t.Status switch
            {
                "Approved" => "#2e7d32",
                "Rejected" => "#c62828",
                "Submitted" => "#1565c0",
                _ => "#555"
            };

            var rowsHtml = string.Empty;
            if (t.Entries != null)
            {
                foreach (var entry in t.Entries.OrderBy(e => e.Date))
                {
                    var dayOfWeek = entry.Date.ToString("ddd");
                    var dateStr = entry.Date.ToString("MMM dd, yyyy");
                    var statusBadge = GetStatusBadge(entry.AttendanceStatus);
                    var tasks = System.Web.HttpUtility.HtmlEncode(entry.TasksCompleted ?? "—");
                    var notes = System.Web.HttpUtility.HtmlEncode(entry.Notes ?? "");
                    var hoursStr = entry.HoursWorked > 0 ? $"{entry.HoursWorked:F1}h" : "—";

                    rowsHtml += $@"
          <tr>
            <td>{dayOfWeek}, {dateStr}</td>
            <td>{statusBadge}</td>
            <td>{hoursStr}</td>
            <td>{tasks}</td>
            <td>{notes}</td>
          </tr>";
                }
            }

            var rejectionRow = t.Status == "Rejected" && !string.IsNullOrWhiteSpace(t.RejectionReason)
                ? $"<div class=\"alert alert-danger\"><strong>Rejection Reason:</strong> {System.Web.HttpUtility.HtmlEncode(t.RejectionReason)}</div>"
                : string.Empty;

            var approvalRow = t.Status == "Approved"
                ? $"<div class=\"alert alert-success\"><strong>Approved by:</strong> {System.Web.HttpUtility.HtmlEncode(approvedByName)} on {t.ApprovedAt:MMM dd, yyyy}" +
                  (string.IsNullOrWhiteSpace(t.ApprovalComment) ? "" : $" — {System.Web.HttpUtility.HtmlEncode(t.ApprovalComment)}") + "</div>"
                : string.Empty;

            var lockedBadge = t.IsLocked
                ? $"<span style=\"background:#37474f;color:#fff;padding:2px 8px;border-radius:4px;font-size:11px;margin-left:8px\">LOCKED</span>"
                : string.Empty;

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<title>Timesheet – {period}</title>
<style>
  *{{box-sizing:border-box;margin:0;padding:0}}
  body{{font-family:'Segoe UI',Arial,sans-serif;background:#f4f6fb;color:#1a1a2e;font-size:13px}}
  .page{{max-width:900px;margin:24px auto;background:#fff;border-radius:8px;box-shadow:0 2px 12px rgba(0,0,0,.12);overflow:hidden}}
  .banner{{background:linear-gradient(135deg,#1565c0 0%,#0d47a1 100%);color:#fff;padding:24px 32px}}
  .banner h1{{font-size:26px;letter-spacing:1px;font-weight:700}}
  .banner .sub{{opacity:.85;margin-top:4px;font-size:12px}}
  .body{{padding:24px 32px}}
  .meta-grid{{display:grid;grid-template-columns:repeat(3,1fr);gap:12px;margin-bottom:20px}}
  .meta-box{{background:#f8fafc;border-left:3px solid #1565c0;border-radius:4px;padding:10px 14px}}
  .meta-box label{{font-size:10px;text-transform:uppercase;color:#666;letter-spacing:.5px}}
  .meta-box p{{font-weight:600;margin-top:2px;color:#1a1a2e}}
  .summary-grid{{display:grid;grid-template-columns:repeat(5,1fr);gap:10px;margin-bottom:20px}}
  .stat-box{{text-align:center;background:#f0f4ff;border-radius:6px;padding:12px 8px}}
  .stat-box .num{{font-size:22px;font-weight:700;color:#1565c0}}
  .stat-box .lbl{{font-size:10px;text-transform:uppercase;color:#666;margin-top:2px}}
  h3{{font-size:12px;text-transform:uppercase;letter-spacing:.8px;color:#1565c0;margin-bottom:8px;padding-bottom:4px;border-bottom:1px solid #e3eaf4}}
  table{{width:100%;border-collapse:collapse;margin-bottom:20px;font-size:12px}}
  th{{background:#1565c0;color:#fff;padding:8px 10px;text-align:left;font-size:11px;text-transform:uppercase;letter-spacing:.5px}}
  td{{padding:7px 10px;border-bottom:1px solid #f0f4f9;vertical-align:top}}
  tr:hover td{{background:#f8fafc}}
  .badge{{display:inline-block;padding:2px 8px;border-radius:10px;font-size:11px;font-weight:600}}
  .badge-present{{background:#e8f5e9;color:#2e7d32}}
  .badge-late{{background:#fff8e1;color:#f57f17}}
  .badge-leave{{background:#e3f2fd;color:#1565c0}}
  .badge-absent{{background:#ffebee;color:#c62828}}
  .badge-remote{{background:#f3e5f5;color:#6a1b9a}}
  .badge-other{{background:#f5f5f5;color:#555}}
  .alert{{padding:10px 14px;border-radius:4px;margin-bottom:16px;font-size:12px}}
  .alert-danger{{background:#ffebee;border-left:3px solid #c62828;color:#c62828}}
  .alert-success{{background:#e8f5e9;border-left:3px solid #2e7d32;color:#2e7d32}}
  .footer{{background:#f8fafc;padding:14px 32px;font-size:11px;color:#888;border-top:1px solid #e3eaf4;display:flex;justify-content:space-between}}
  @media print{{body{{background:#fff}}.page{{box-shadow:none}}}}
</style>
</head>
<body>
<div class=""page"">
  <div class=""banner"">
    <h1>TIMESHEET{lockedBadge}</h1>
    <div class=""sub"">Period: {period} &nbsp;|&nbsp; Status: <strong style=""color:#fff"">{t.Status.ToUpper()}</strong></div>
  </div>
  <div class=""body"">
    <div class=""meta-grid"">
      <div class=""meta-box""><label>Employee Name</label><p>{System.Web.HttpUtility.HtmlEncode(empName)}</p></div>
      <div class=""meta-box""><label>Employee ID</label><p>{System.Web.HttpUtility.HtmlEncode(empId)}</p></div>
      <div class=""meta-box""><label>Department</label><p>{System.Web.HttpUtility.HtmlEncode(dept)}</p></div>
      <div class=""meta-box""><label>Position</label><p>{System.Web.HttpUtility.HtmlEncode(pos)}</p></div>
      <div class=""meta-box""><label>Pay Period</label><p>{period}</p></div>
      <div class=""meta-box""><label>Generated On</label><p>{t.CreatedAt:MMM dd, yyyy}</p></div>
    </div>

    <h3>Attendance Summary</h3>
    <div class=""summary-grid"">
      <div class=""stat-box""><div class=""num"">{t.TotalWorkingDays}</div><div class=""lbl"">Working Days</div></div>
      <div class=""stat-box""><div class=""num"" style=""color:#2e7d32"">{t.PresentDays}</div><div class=""lbl"">Present</div></div>
      <div class=""stat-box""><div class=""num"" style=""color:#f57f17"">{t.LateDays}</div><div class=""lbl"">Late</div></div>
      <div class=""stat-box""><div class=""num"" style=""color:#1565c0"">{t.OnLeaveDays}</div><div class=""lbl"">On Leave</div></div>
      <div class=""stat-box""><div class=""num"" style=""color:#c62828"">{t.AbsentDays}</div><div class=""lbl"">Absent</div></div>
    </div>
    <p style=""font-size:12px;color:#555;margin-bottom:20px"">Total Hours Worked: <strong>{t.TotalHours:F1}h</strong></p>

    {rejectionRow}{approvalRow}

    <h3>Daily Entries</h3>
    <table>
      <thead>
        <tr>
          <th style=""width:160px"">Date</th>
          <th style=""width:100px"">Status</th>
          <th style=""width:70px"">Hours</th>
          <th>Tasks Completed</th>
          <th style=""width:180px"">Notes</th>
        </tr>
      </thead>
      <tbody>
        {rowsHtml}
      </tbody>
    </table>
  </div>
  <div class=""footer"">
    <span>Generated: {DateTime.Now:MMM dd, yyyy HH:mm}</span>
    <span>This is a computer-generated document.</span>
  </div>
</div>
</body></html>";
        }

        private static string GetStatusBadge(string? status) => status?.ToLower() switch
        {
            "present" => "<span class=\"badge badge-present\">Present</span>",
            "late" => "<span class=\"badge badge-late\">Late</span>",
            "on leave" => "<span class=\"badge badge-leave\">On Leave</span>",
            "absent" => "<span class=\"badge badge-absent\">Absent</span>",
            "remote" => "<span class=\"badge badge-remote\">Remote</span>",
            _ => $"<span class=\"badge badge-other\">{System.Web.HttpUtility.HtmlEncode(status ?? "—")}</span>",
        };
    }
}
