using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Services
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAttendancesAsync(int? employeeId, DateTime? startDate, DateTime? endDate);
        Task<Attendance> CreateAttendanceAsync(Attendance attendance);
        Task UpdateAttendanceAsync(int id, Attendance attendance);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ILogger<AttendanceService> _logger;
        private readonly AttendanceSettings _attendanceSettings;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            ILogger<AttendanceService> logger,
            IConfiguration configuration)
        {
            _attendanceRepository = attendanceRepository;
            _logger = logger;
            _attendanceSettings = configuration.GetSection("AttendanceSettings").Get<AttendanceSettings>()
                ?? new AttendanceSettings();
        }

        public async Task<IEnumerable<Attendance>> GetAttendancesAsync(int? employeeId, DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation(
                "Retrieving attendance records. EmployeeId: {EmployeeId}, StartDate: {StartDate}, EndDate: {EndDate}",
                employeeId, startDate, endDate);

            await AutoCloseStaleAttendancesAsync(employeeId);

            var attendances = await _attendanceRepository.GetByDateRangeAsync(employeeId, startDate, endDate);

            _logger.LogInformation("Retrieved {Count} attendance records.", attendances.Count());
            return attendances.OrderByDescending(a => a.Date);
        }

        public async Task<Attendance> CreateAttendanceAsync(Attendance attendance)
        {
            _logger.LogInformation("Creating attendance record for employee {EmployeeId}.", attendance.EmployeeId);

            await AutoCloseStaleAttendancesAsync(attendance.EmployeeId);

            var today = DateTime.Today;
            var todayAttendances = await _attendanceRepository.GetByDateRangeAsync(attendance.EmployeeId, today, today);
            var hasOpenShift = todayAttendances.Any(a => !a.CheckOut.HasValue);

            if (hasOpenShift)
            {
                _logger.LogInformation(
                    "Rejected duplicate check-in for employee {EmployeeId} due to existing open shift.",
                    attendance.EmployeeId);
                throw new InvalidOperationException("You already have an open shift for today. Please check out first.");
            }

            attendance.Date = DateTime.Today;
            attendance.CheckIn = DateTime.UtcNow;
            attendance.Status = IsLateArrival(DateTime.Now.TimeOfDay) ? "Late" : "Present";
            attendance.CreatedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            var createdAttendance = await _attendanceRepository.AddAsync(attendance);

            _logger.LogInformation("Attendance record created with id {AttendanceId}.", createdAttendance.Id);
            return createdAttendance;
        }

        public async Task UpdateAttendanceAsync(int id, Attendance attendance)
        {
            _logger.LogInformation("Updating attendance record with id {AttendanceId}.", id);

            if (id != attendance.Id)
            {
                _logger.LogInformation($"Attendance id mismatch. Route id {id}, body id {attendance.Id}.");
                throw new ArgumentException("Attendance id mismatch.");
            }

            var existingAttendance = await _attendanceRepository.GetByIdAsync(id);
            if (existingAttendance == null)
            {
                _logger.LogInformation($"Attendance record with id {id} not found.");
                throw new KeyNotFoundException($"Attendance record with id {id} not found.");
            }

            existingAttendance.CheckOut = attendance.CheckOut;
            existingAttendance.Notes = attendance.Notes;
            existingAttendance.Location = string.IsNullOrWhiteSpace(attendance.Location)
                ? existingAttendance.Location
                : attendance.Location;

            if (attendance.BreakDuration >= 0)
            {
                existingAttendance.BreakDuration = attendance.BreakDuration;
            }

            if (existingAttendance.CheckOut.HasValue && existingAttendance.CheckIn != default)
            {
                var duration = existingAttendance.CheckOut.Value - existingAttendance.CheckIn;
                var workedHours = (decimal)duration.TotalHours - (existingAttendance.BreakDuration / 60m);
                existingAttendance.TotalHours = workedHours < 0 ? 0 : workedHours;
                existingAttendance.Status = existingAttendance.Status;
                _logger.LogInformation($"Calculated total hours {existingAttendance.TotalHours} for attendance id {id}.");
            }
            else
            {
                existingAttendance.Status = string.IsNullOrWhiteSpace(attendance.Status)
                    ? existingAttendance.Status
                    : attendance.Status;
            }

            existingAttendance.UpdatedAt = DateTime.Now;
            await _attendanceRepository.UpdateAsync(existingAttendance);

            _logger.LogInformation($"Attendance record with id {id} updated successfully.");
        }

        private bool IsLateArrival(TimeSpan checkInTime)
        {
            if (!TimeSpan.TryParse(_attendanceSettings.WorkDayStartTime, out var workStart))
                workStart = TimeSpan.FromHours(8);
            return checkInTime > workStart;
        }

        private async Task AutoCloseStaleAttendancesAsync(int? employeeId = null)
        {
            if (!TimeSpan.TryParse(_attendanceSettings.WorkDayEndTime, out var workEnd))
                workEnd = TimeSpan.FromHours(17);

            var today = DateTime.Today;
            var staleAttendances = await _attendanceRepository.GetOpenAttendancesBeforeDateAsync(today, employeeId);

            foreach (var staleAttendance in staleAttendances)
            {
                var autoCheckOut = staleAttendance.Date.Date.Add(workEnd);
                staleAttendance.CheckOut = autoCheckOut;
                staleAttendance.Status = staleAttendance.Status;

                if (staleAttendance.CheckIn != default)
                {
                    var duration = autoCheckOut - staleAttendance.CheckIn;
                    var workedHours = (decimal)duration.TotalHours - (staleAttendance.BreakDuration / 60m);
                    staleAttendance.TotalHours = workedHours < 0 ? 0 : workedHours;
                }

                staleAttendance.UpdatedAt = DateTime.Now;

                await _attendanceRepository.UpdateAsync(staleAttendance);

                _logger.LogInformation(
                    "Auto-closed stale attendance {AttendanceId} for EmployeeId={EmployeeId} with checkout at {CheckOut}",
                    staleAttendance.Id, staleAttendance.EmployeeId, autoCheckOut);
            }
        }
    }

    public class AttendanceSettings
    {
        public string WorkDayStartTime { get; set; } = "08:00";
        public string WorkDayEndTime { get; set; } = "17:00";
    }
}