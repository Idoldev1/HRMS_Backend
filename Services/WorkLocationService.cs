using HRMS.API.Common;
using HRMS.API.Models;
using HRMS.API.Repositories;

namespace HRMS.API.Services
{
    public interface IWorkLocationService
    {
        Task<Result<IEnumerable<WorkLocation>>> GetAllLocationsAsync();
        Task<Result<IEnumerable<WorkLocation>>> GetActiveLocationsAsync();
        Task<Result<WorkLocation>> GetLocationAsync(int id);
        Task<Result<WorkLocation>> CreateLocationAsync(WorkLocation location);
        Task<Result> UpdateLocationAsync(int id, WorkLocation location);
        Task<Result> DeleteLocationAsync(int id);
        Task<Result<IEnumerable<WorkLocation>>> GetEmployeeLocationsAsync(string employeeId);
        Task<Result<IEnumerable<EmployeeWorkLocation>>> GetEmployeeAssignmentsAsync(string employeeId);
        Task<Result> AssignLocationToEmployeeAsync(string employeeId, int workLocationId);
        Task<Result> RemoveLocationFromEmployeeAsync(string employeeId, int workLocationId);
    }

    public class WorkLocationService : IWorkLocationService
    {
        private readonly IWorkLocationRepository _workLocationRepository;
        private readonly IEmployeeWorkLocationRepository _employeeWorkLocationRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<WorkLocationService> _logger;

        public WorkLocationService(
            IWorkLocationRepository workLocationRepository,
            IEmployeeWorkLocationRepository employeeWorkLocationRepository,
            IEmployeeRepository employeeRepository,
            ILogger<WorkLocationService> logger)
        {
            _workLocationRepository = workLocationRepository;
            _employeeWorkLocationRepository = employeeWorkLocationRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<WorkLocation>>> GetAllLocationsAsync()
        {
            _logger.LogInformation("Fetching all work locations");
            var locations = await _workLocationRepository.GetAllAsync();
            _logger.LogInformation("Returned {Count} work locations", locations.Count());
            return Result<IEnumerable<WorkLocation>>.Ok(locations);
        }

        public async Task<Result<IEnumerable<WorkLocation>>> GetActiveLocationsAsync()
        {
            _logger.LogInformation("Fetching active work locations");
            var locations = await _workLocationRepository.GetActiveLocationsAsync();
            _logger.LogInformation("Returned {Count} active work locations", locations.Count());
            return Result<IEnumerable<WorkLocation>>.Ok(locations);
        }

        public async Task<Result<WorkLocation>> GetLocationAsync(int id)
        {
            _logger.LogInformation("Fetching work location {WorkLocationId}", id);
            var location = await _workLocationRepository.GetByIdAsync(id);
            if (location is null)
            {
                _logger.LogWarning("Work location {WorkLocationId} not found", id);
                return Result<WorkLocation>.NotFound($"Work location with id {id} not found.");
            }
            return Result<WorkLocation>.Ok(location);
        }

        public async Task<Result<WorkLocation>> CreateLocationAsync(WorkLocation location)
        {
            _logger.LogInformation(
                "Creating work location '{Name}' at ({Latitude}, {Longitude}), radius: {Radius}m",
                location.Name, location.Latitude, location.Longitude, location.AllowedRadiusMeters);

            location.CreatedAt = DateTime.UtcNow;
            location.UpdatedAt = DateTime.UtcNow;

            var created = await _workLocationRepository.AddAsync(location);

            _logger.LogInformation("Work location '{Name}' created with id {WorkLocationId}",
                created.Name, created.Id);

            return Result<WorkLocation>.Ok(created);
        }

        public async Task<Result> UpdateLocationAsync(int id, WorkLocation location)
        {
            _logger.LogInformation("Updating work location {WorkLocationId}", id);

            var existing = await _workLocationRepository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Work location {WorkLocationId} not found for update", id);
                return Result.NotFound($"Work location with id {id} not found.");
            }

            existing.Name = location.Name;
            existing.Address = location.Address;
            existing.Latitude = location.Latitude;
            existing.Longitude = location.Longitude;
            existing.AllowedRadiusMeters = location.AllowedRadiusMeters;
            existing.IsActive = location.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            await _workLocationRepository.UpdateAsync(existing);

            _logger.LogInformation(
                "Work location {WorkLocationId} updated. Name: '{Name}', Radius: {Radius}m, Active: {IsActive}",
                id, existing.Name, existing.AllowedRadiusMeters, existing.IsActive);

            return Result.Ok();
        }

        public async Task<Result> DeleteLocationAsync(int id)
        {
            _logger.LogInformation("Deleting work location {WorkLocationId}", id);

            var existing = await _workLocationRepository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Work location {WorkLocationId} not found for deletion", id);
                return Result.NotFound($"Work location with id {id} not found.");
            }

            await _workLocationRepository.DeleteAsync(existing);

            _logger.LogInformation("Work location {WorkLocationId} ('{Name}') deleted", id, existing.Name);
            return Result.Ok();
        }

        public async Task<Result<IEnumerable<WorkLocation>>> GetEmployeeLocationsAsync(string employeeId)
        {
            _logger.LogInformation("Fetching assigned work locations for employee {EmployeeId}", employeeId);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for work location lookup", employeeId);
                return Result<IEnumerable<WorkLocation>>.NotFound($"Employee '{employeeId}' not found.");
            }

            var locations = await _workLocationRepository.GetByEmployeeIdAsync(employee.Id);

            _logger.LogInformation("Employee {EmployeeId} has {Count} assigned active work locations",
                employeeId, locations.Count());

            return Result<IEnumerable<WorkLocation>>.Ok(locations);
        }

        public async Task<Result<IEnumerable<EmployeeWorkLocation>>> GetEmployeeAssignmentsAsync(string employeeId)
        {
            _logger.LogInformation("Fetching all work location assignments for employee {EmployeeId}", employeeId);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for assignment lookup", employeeId);
                return Result<IEnumerable<EmployeeWorkLocation>>.NotFound($"Employee '{employeeId}' not found.");
            }

            var assignments = await _employeeWorkLocationRepository.GetByEmployeeIdAsync(employee.Id);

            _logger.LogInformation("Employee {EmployeeId} has {Count} total work location assignments",
                employeeId, assignments.Count());

            return Result<IEnumerable<EmployeeWorkLocation>>.Ok(assignments);
        }

        public async Task<Result> AssignLocationToEmployeeAsync(string employeeId, int workLocationId)
        {
            _logger.LogInformation(
                "Assigning work location {WorkLocationId} to employee {EmployeeId}", workLocationId, employeeId);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for work location assignment", employeeId);
                return Result.NotFound($"Employee '{employeeId}' not found.");
            }

            var location = await _workLocationRepository.GetByIdAsync(workLocationId);
            if (location is null)
            {
                _logger.LogWarning("Work location {WorkLocationId} not found for assignment", workLocationId);
                return Result.NotFound($"Work location with id {workLocationId} not found.");
            }

            var existing = await _employeeWorkLocationRepository.GetAssignmentAsync(employee.Id, workLocationId);
            if (existing is not null)
            {
                if (existing.IsActive)
                {
                    _logger.LogWarning(
                        "Employee {EmployeeId} is already assigned to work location {WorkLocationId}",
                        employeeId, workLocationId);
                    return Result.Conflict("Employee is already assigned to this work location.");
                }

                existing.IsActive = true;
                existing.AssignedAt = DateTime.UtcNow;
                await _employeeWorkLocationRepository.UpdateAsync(existing);
            }
            else
            {
                await _employeeWorkLocationRepository.AddAsync(new EmployeeWorkLocation
                {
                    EmployeeId = employee.Id,
                    WorkLocationId = workLocationId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                });
            }

            _logger.LogInformation(
                "Employee {EmployeeId} successfully assigned to work location {WorkLocationId} ('{LocationName}')",
                employeeId, workLocationId, location.Name);

            return Result.Ok();
        }

        public async Task<Result> RemoveLocationFromEmployeeAsync(string employeeId, int workLocationId)
        {
            _logger.LogInformation(
                "Removing work location {WorkLocationId} from employee {EmployeeId}", workLocationId, employeeId);

            var employee = await _employeeRepository.GetByEmployeeIdAsync(employeeId);
            if (employee is null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for work location removal", employeeId);
                return Result.NotFound($"Employee '{employeeId}' not found.");
            }

            var assignment = await _employeeWorkLocationRepository.GetAssignmentAsync(employee.Id, workLocationId);
            if (assignment is null)
            {
                _logger.LogWarning(
                    "No assignment found for employee {EmployeeId} and work location {WorkLocationId}",
                    employeeId, workLocationId);
                return Result.NotFound("No assignment found between this employee and work location.");
            }

            if (!assignment.IsActive)
            {
                _logger.LogWarning(
                    "Assignment for employee {EmployeeId} to work location {WorkLocationId} is already inactive",
                    employeeId, workLocationId);
                return Result.Conflict("This assignment is already inactive.");
            }

            assignment.IsActive = false;
            await _employeeWorkLocationRepository.UpdateAsync(assignment);

            _logger.LogInformation(
                "Employee {EmployeeId} unassigned from work location {WorkLocationId}",
                employeeId, workLocationId);

            return Result.Ok();
        }
    }
}
