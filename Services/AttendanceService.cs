using HRMS.API.Models;
using HRMS.API.Repositories;
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

        public AttendanceService(IAttendanceRepository attendanceRepository, ILogger<AttendanceService> logger)
        {
            _attendanceRepository = attendanceRepository;
            _logger = logger;
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
                existingAttendance.Status = "Signed Out";
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

        private async Task AutoCloseStaleAttendancesAsync(int? employeeId = null)
        {
            var today = DateTime.Today;
            var staleAttendances = await _attendanceRepository.GetOpenAttendancesBeforeDateAsync(today, employeeId);

            foreach (var staleAttendance in staleAttendances)
            {
                var dayEnd = staleAttendance.Date.Date.AddDays(1).AddSeconds(-1);
                staleAttendance.CheckOut = dayEnd;

                if (staleAttendance.CheckIn != default)
                {
                    var duration = dayEnd - staleAttendance.CheckIn;
                    var workedHours = (decimal)duration.TotalHours - (staleAttendance.BreakDuration / 60m);
                    staleAttendance.TotalHours = workedHours < 0 ? 0 : workedHours;
                }

                staleAttendance.Status = "Signed Out";
                staleAttendance.UpdatedAt = DateTime.Now;

                await _attendanceRepository.UpdateAsync(staleAttendance);

                _logger.LogInformation(
                    $"Auto-closed stale attendance id {staleAttendance.Id} for employee {staleAttendance.EmployeeId}.");
            }
        }
    }
}

