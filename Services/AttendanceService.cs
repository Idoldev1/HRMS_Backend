using HRMS.API.Common;
using HRMS.API.Contracts.Attendance;
using HRMS.API.Models;
using HRMS.API.Repositories;

namespace HRMS.API.Services
{
    public interface IAttendanceService
    {
        Task<Result<IEnumerable<Attendance>>> GetAttendancesAsync(string? employeeId, DateTime? startDate, DateTime? endDate);
        Task<Result<Attendance>> CreateAttendanceAsync(string employeeId, MarkAttendanceRequest request);
        Task<Result> UpdateAttendanceAsync(int id, Attendance attendance);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IWorkLocationRepository _workLocationRepository;
        private readonly IEmployeeDeviceRepository _deviceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<AttendanceService> _logger;
        private readonly AttendanceSettings _settings;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IWorkLocationRepository workLocationRepository,
            IEmployeeDeviceRepository deviceRepository,
            IEmployeeRepository employeeRepository,
            ILogger<AttendanceService> logger,
            IConfiguration configuration)
        {
            _attendanceRepository = attendanceRepository;
            _workLocationRepository = workLocationRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
            _settings = configuration.GetSection("AttendanceSettings").Get<AttendanceSettings>()
                ?? new AttendanceSettings();
        }

        public async Task<Result<IEnumerable<Attendance>>> GetAttendancesAsync(
            string? employeeId, DateTime? startDate, DateTime? endDate)
        {
            _logger.LogInformation(
                "Fetching attendance records. EmployeeId: {EmployeeId}, StartDate: {StartDate}, EndDate: {EndDate}",
                employeeId, startDate, endDate);

            int? resolvedId = null;
            if (employeeId is not null)
            {
                var emp = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
                if (emp is null)
                {
                    _logger.LogWarning("GetAttendances: employee {EmployeeId} not found", employeeId);
                    return Result<IEnumerable<Attendance>>.NotFound($"Employee '{employeeId}' not found.");
                }
                resolvedId = emp.Id;
            }

            await AutoCloseStaleAttendancesAsync(resolvedId);

            var records = await _attendanceRepository.GetByDateRangeAsync(resolvedId, startDate, endDate);
            var ordered = records.OrderByDescending(a => a.Date).ToList();

            _logger.LogInformation("Returned {Count} attendance records for employee {EmployeeId}",
                ordered.Count, employeeId);

            return Result<IEnumerable<Attendance>>.Ok(ordered);
        }

        public async Task<Result<Attendance>> CreateAttendanceAsync(string employeeId, MarkAttendanceRequest request)
        {
            _logger.LogInformation(
                "Processing check-in for employee {EmployeeId}. DeviceId: {DeviceId}, Lat: {Latitude}, Lon: {Longitude}",
                employeeId, request.DeviceId, request.Latitude, request.Longitude);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Check-in rejected: employee {EmployeeId} not found", employeeId);
                return Result<Attendance>.NotFound($"Employee '{employeeId}' not found.");
            }

            var numericId = employee.Id;
            await AutoCloseStaleAttendancesAsync(numericId);

            // --- Device validation ---
            var device = await _deviceRepository.GetByDeviceIdAndEmployeeAsync(request.DeviceId, numericId);
            if (device is null)
            {
                _logger.LogWarning(
                    "Check-in rejected for employee {EmployeeId}: device {DeviceId} is not registered",
                    employeeId, request.DeviceId);
                return Result<Attendance>.Fail("This device is not registered. Please register your device or contact HR.");
            }

            if (!device.IsActive)
            {
                _logger.LogWarning(
                    "Check-in rejected for employee {EmployeeId}: device {DeviceId} is inactive",
                    employeeId, request.DeviceId);
                return Result<Attendance>.Fail("This device has been deactivated. Contact HR to reactivate it.");
            }

            // --- Work location check ---
            var assignedLocations = (await _workLocationRepository.GetByEmployeeIdAsync(numericId)).ToList();
            if (assignedLocations.Count == 0)
            {
                _logger.LogWarning(
                    "Check-in rejected for employee {EmployeeId}: no active work locations assigned", employeeId);
                return Result<Attendance>.Fail("No active work locations are assigned to you. Contact HR.");
            }

            // --- Geofencing: Haversine against each assigned location ---
            WorkLocation? matchedLocation = null;
            double closestDistance = double.MaxValue;

            foreach (var location in assignedLocations)
            {
                double distance = CalculateDistanceMeters(
                    (double)request.Latitude, (double)request.Longitude,
                    (double)location.Latitude, (double)location.Longitude);

                _logger.LogDebug(
                    "Employee {EmployeeId} is {Distance:F1}m from '{LocationName}' (allowed: {Radius}m)",
                    employeeId, distance, location.Name, location.AllowedRadiusMeters);

                if (distance < closestDistance)
                    closestDistance = distance;

                if (distance <= location.AllowedRadiusMeters)
                {
                    matchedLocation = location;
                    break;
                }
            }

            if (matchedLocation is null)
            {
                _logger.LogWarning(
                    "Check-in rejected for employee {EmployeeId}: outside all approved locations. Closest: {Distance:F0}m",
                    employeeId, closestDistance);
                return Result<Attendance>.Fail(
                    $"You are not within any approved work location. Closest site is {closestDistance:F0}m away.");
            }

            // --- Duplicate check-in guard ---
            var today = DateTime.Today;
            var todayRecords = await _attendanceRepository.GetByDateRangeAsync(numericId, today, today);
            if (todayRecords.Any(a => !a.CheckOut.HasValue))
            {
                _logger.LogWarning(
                    "Duplicate check-in attempt by employee {EmployeeId} on {Date}", employeeId, today);
                return Result<Attendance>.Conflict("You already have an open shift for today. Please check out first.");
            }

            // --- Create record ---
            var checkInTime = DateTime.Now.TimeOfDay;
            var attendance = new Attendance
            {
                EmployeeId = numericId,
                Date = today,
                CheckIn = DateTime.UtcNow,
                Status = IsLateArrival(checkInTime) ? "Late" : "Present",
                Location = matchedLocation.Name,
                DeviceId = request.DeviceId,
                CheckInLatitude = request.Latitude,
                CheckInLongitude = request.Longitude,
                WorkLocationId = matchedLocation.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var created = await _attendanceRepository.AddAsync(attendance);

            device.LastUsedAt = DateTime.UtcNow;
            await _deviceRepository.UpdateAsync(device);

            _logger.LogInformation(
                "Check-in successful. AttendanceId: {AttendanceId}, EmployeeId: {EmployeeId}, Location: '{Location}', Status: {Status}",
                created.Id, employeeId, matchedLocation.Name, created.Status);

            return Result<Attendance>.Ok(created);
        }

        public async Task<Result> UpdateAttendanceAsync(int id, Attendance attendance)
        {
            _logger.LogInformation("Updating attendance record {AttendanceId}", id);

            if (id != attendance.Id)
            {
                _logger.LogWarning(
                    "Attendance id mismatch. Route id: {RouteId}, Body id: {BodyId}", id, attendance.Id);
                return Result.Fail("Attendance id in the URL does not match the body.");
            }

            var existing = await _attendanceRepository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Attendance record {AttendanceId} not found for update", id);
                return Result.NotFound($"Attendance record with id {id} not found.");
            }

            existing.CheckOut = attendance.CheckOut;
            existing.Notes = attendance.Notes;

            if (attendance.BreakDuration >= 0)
                existing.BreakDuration = attendance.BreakDuration;

            if (existing.CheckOut.HasValue && existing.CheckIn != default)
            {
                var duration = existing.CheckOut.Value - existing.CheckIn;
                var workedHours = (decimal)duration.TotalHours - (existing.BreakDuration / 60m);
                existing.TotalHours = workedHours < 0 ? 0 : workedHours;
            }

            existing.UpdatedAt = DateTime.UtcNow;
            await _attendanceRepository.UpdateAsync(existing);

            _logger.LogInformation(
                "Attendance record {AttendanceId} updated. TotalHours: {TotalHours}, CheckOut: {CheckOut}",
                id, existing.TotalHours, existing.CheckOut);

            return Result.Ok();
        }

        private bool IsLateArrival(TimeSpan checkInTime)
        {
            if (!TimeSpan.TryParse(_settings.WorkDayStartTime, out var workStart))
                workStart = TimeSpan.FromHours(8);
            return checkInTime > workStart;
        }

        private async Task AutoCloseStaleAttendancesAsync(int? numericEmployeeId = null)
        {
            if (!TimeSpan.TryParse(_settings.WorkDayEndTime, out var workEnd))
                workEnd = TimeSpan.FromHours(17);

            var stale = await _attendanceRepository.GetOpenAttendancesBeforeDateAsync(DateTime.Today, numericEmployeeId);

            foreach (var record in stale)
            {
                var autoCheckOut = record.Date.Date.Add(workEnd);
                record.CheckOut = autoCheckOut;

                if (record.CheckIn != default)
                {
                    var duration = autoCheckOut - record.CheckIn;
                    var workedHours = (decimal)duration.TotalHours - (record.BreakDuration / 60m);
                    record.TotalHours = workedHours < 0 ? 0 : workedHours;
                }

                record.UpdatedAt = DateTime.UtcNow;
                await _attendanceRepository.UpdateAsync(record);

                _logger.LogInformation(
                    "Auto-closed stale attendance {AttendanceId} for employee {EmployeeId}. AutoCheckOut: {CheckOut}",
                    record.Id, record.EmployeeId, autoCheckOut);
            }
        }

        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6_371_000;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                    * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }

    public class AttendanceSettings
    {
        public string WorkDayStartTime { get; set; } = "08:00";
        public string WorkDayEndTime { get; set; } = "17:00";
    }
}
