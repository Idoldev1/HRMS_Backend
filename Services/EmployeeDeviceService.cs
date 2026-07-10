using HRMS.API.Common;
using HRMS.API.Models;
using HRMS.API.Repositories;

namespace HRMS.API.Services
{
    public interface IEmployeeDeviceService
    {
        Task<Result<IEnumerable<EmployeeDevice>>> GetDevicesAsync(string? employeeId);
        Task<Result<EmployeeDevice>> GetDeviceAsync(int id);
        Task<Result<EmployeeDevice>> RegisterDeviceAsync(string employeeId, EmployeeDevice device);
        Task<Result> ToggleDeviceStatusAsync(int id);
        Task<Result> DeleteDeviceAsync(int id);
    }

    public class EmployeeDeviceService : IEmployeeDeviceService
    {
        private readonly IEmployeeDeviceRepository _deviceRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeDeviceService> _logger;

        public EmployeeDeviceService(
            IEmployeeDeviceRepository deviceRepository,
            IEmployeeRepository employeeRepository,
            ILogger<EmployeeDeviceService> logger)
        {
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<EmployeeDevice>>> GetDevicesAsync(string? employeeId)
        {
            _logger.LogInformation("Fetching devices. EmployeeId filter: {EmployeeId}", employeeId);

            IEnumerable<EmployeeDevice> devices;

            if (employeeId is not null)
            {
                var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
                if (employee is null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found for device listing", employeeId);
                    return Result<IEnumerable<EmployeeDevice>>.NotFound($"Employee '{employeeId}' not found.");
                }
                devices = await _deviceRepository.GetByEmployeeIdAsync(employee.Id);
            }
            else
            {
                devices = await _deviceRepository.GetAllAsync();
            }

            _logger.LogInformation("Returned {Count} devices for EmployeeId filter {EmployeeId}",
                devices.Count(), employeeId);

            return Result<IEnumerable<EmployeeDevice>>.Ok(devices);
        }

        public async Task<Result<EmployeeDevice>> GetDeviceAsync(int id)
        {
            _logger.LogInformation("Fetching device record {DeviceRecordId}", id);

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device is null)
            {
                _logger.LogWarning("Device record {DeviceRecordId} not found", id);
                return Result<EmployeeDevice>.NotFound($"Device not found.");
            }

            return Result<EmployeeDevice>.Ok(device);
        }

        public async Task<Result<EmployeeDevice>> RegisterDeviceAsync(string employeeId, EmployeeDevice device)
        {
            _logger.LogInformation(
                "Registering device '{DeviceName}' (DeviceId: {DeviceId}) for employee {EmployeeId}",
                device.DeviceName, device.DeviceId, employeeId);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found during device registration", employeeId);
                return Result<EmployeeDevice>.NotFound($"Employee '{employeeId}' not found.");
            }

            var alreadyExists = await _deviceRepository.DeviceIdExistsForEmployeeAsync(device.DeviceId, employee.Id);
            if (alreadyExists)
            {
                _logger.LogWarning(
                    "Device {DeviceId} is already registered for employee {EmployeeId}",
                    device.DeviceId, employeeId);
                return Result<EmployeeDevice>.Conflict("This device is already registered for the employee.");
            }

            device.EmployeeId = employee.Id;
            device.IsActive = true;
            device.RegisteredAt = DateTime.UtcNow;

            var registered = await _deviceRepository.AddAsync(device);

            _logger.LogInformation(
                "Device '{DeviceName}' (id: {DeviceRecordId}) registered for employee {EmployeeId}",
                registered.DeviceName, registered.Id, employeeId);

            return Result<EmployeeDevice>.Ok(registered);
        }

        public async Task<Result> ToggleDeviceStatusAsync(int id)
        {
            _logger.LogInformation("Toggling status for device record {DeviceRecordId}", id);

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device is null)
            {
                _logger.LogWarning("Device record {DeviceRecordId} not found for status toggle", id);
                return Result.NotFound($"Device with id {id} not found.");
            }

            var previousStatus = device.IsActive;
            device.IsActive = !device.IsActive;
            await _deviceRepository.UpdateAsync(device);

            _logger.LogInformation(
                "Device {DeviceRecordId} ('{DeviceName}') toggled from {PreviousStatus} to {NewStatus}",
                id, device.DeviceName,
                previousStatus ? "Active" : "Inactive",
                device.IsActive ? "Active" : "Inactive");

            return Result.Ok();
        }

        public async Task<Result> DeleteDeviceAsync(int id)
        {
            _logger.LogInformation("Deleting device record {DeviceRecordId}", id);

            var device = await _deviceRepository.GetByIdAsync(id);
            if (device is null)
            {
                _logger.LogWarning("Device record {DeviceRecordId} not found for deletion", id);
                return Result.NotFound($"Device with id {id} not found.");
            }

            await _deviceRepository.DeleteAsync(device);

            _logger.LogInformation(
                "Device record {DeviceRecordId} ('{DeviceName}', employee {EmployeeId}) deleted",
                id, device.DeviceName, device.EmployeeId);

            return Result.Ok();
        }
    }
}
