using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetDepartmentsAsync();
        Task<Department?> GetDepartmentByIdAsync(int id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(int id, Department department);
        Task SoftDeleteDepartmentAsync(int id);
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger)
        {
            _departmentRepository = departmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Department>> GetDepartmentsAsync()
        {
            _logger.LogInformation("Retrieving all active departments.");

            var departments = await _departmentRepository.FindAsync(d => d.IsActive);

            _logger.LogInformation("Retrieved {Count} departments.", departments.Count());
            return departments.OrderBy(d => d.Name);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving department by id {DepartmentId}.", id);

            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null)
            {
                _logger.LogInformation("Department with id {DepartmentId} not found.", id);
            }

            return department;
        }

        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            _logger.LogInformation("Creating department with name {Name}.", department.Name);

            department.CreatedAt = DateTime.Now;
            department.UpdatedAt = DateTime.Now;

            var createdDepartment = await _departmentRepository.AddAsync(department);

            _logger.LogInformation("Department created with id {DepartmentId}.", createdDepartment.Id);
            return createdDepartment;
        }

        public async Task UpdateDepartmentAsync(int id, Department department)
        {
            _logger.LogInformation("Updating department with id {DepartmentId}.", id);

            if (id != department.Id)
            {
                _logger.LogInformation("Department id mismatch. Route id {RouteId}, body id {BodyId}.", id, department.Id);
                throw new ArgumentException("Department id mismatch.");
            }

            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
            {
                _logger.LogInformation("Failed to update department. Department with id {DepartmentId} not found.", id);
                throw new KeyNotFoundException($"Department with id {id} not found.");
            }

            department.UpdatedAt = DateTime.Now;
            await _departmentRepository.UpdateAsync(department);

            _logger.LogInformation("Department with id {DepartmentId} updated successfully.", id);
        }

        public async Task SoftDeleteDepartmentAsync(int id)
        {
            _logger.LogInformation("Soft deleting department with id {DepartmentId}.", id);

            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                _logger.LogInformation("Cannot delete department. Department with id {DepartmentId} not found.", id);
                throw new KeyNotFoundException($"Department with id {id} not found.");
            }

            department.IsActive = false;
            department.UpdatedAt = DateTime.Now;
            await _departmentRepository.UpdateAsync(department);

            _logger.LogInformation("Department with id {DepartmentId} deactivated successfully.", id);
        }
    }
}

