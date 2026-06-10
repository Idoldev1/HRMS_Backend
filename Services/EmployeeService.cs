using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.Extensions.Logging;

namespace HRMS.API.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetEmployeesAsync(string? department, string? status, string? search);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(int id, Employee employee);
        Task SoftDeleteEmployeeAsync(int id);
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesAsync(string? department, string? status, string? search)
        {
            _logger.LogInformation("Retrieving employees. Department: {Department}, Status: {Status}, Search: {Search}",
                department, status, search);

            IEnumerable<Employee> employees;

            if (!string.IsNullOrWhiteSpace(search))
            {
                employees = await _employeeRepository.SearchAsync(search);
            }
            else if (!string.IsNullOrWhiteSpace(department))
            {
                employees = await _employeeRepository.GetByDepartmentAsync(department);
            }
            else if (!string.IsNullOrWhiteSpace(status))
            {
                employees = await _employeeRepository.GetByStatusAsync(status);
            }
            else
            {
                employees = await _employeeRepository.GetAllWithIncludesAsync();
            }

            // Filter by both department and status if both are provided
            if (!string.IsNullOrWhiteSpace(department) && !string.IsNullOrWhiteSpace(status))
            {
                employees = employees.Where(e => e.Department?.Name == department && e.Status == status);
            }

            _logger.LogInformation("Retrieved {Count} employees.", employees.Count());
            return employees.OrderByDescending(e => e.CreatedAt);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving employee by id {EmployeeId}.", id);

            var employee = await _employeeRepository.GetByIdWithIncludesAsync(id);

            if (employee == null)
            {
                _logger.LogInformation("Employee with id {EmployeeId} not found.", id);
            }

            return employee;
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("Creating employee for email {Email}.", employee.Email);

            if (string.IsNullOrWhiteSpace(employee.EmployeeId))
            {
                var allEmployees = await _employeeRepository.GetAllAsync();
                var count = allEmployees.Count();
                employee.EmployeeId = $"EMP{(count + 1):D5}";
                _logger.LogInformation($"Generated EmployeeId {employee.EmployeeId} for email {employee.Email}.");
            }

            // Always start as Pending until the employee completes onboarding
            employee.Status = "Pending";
            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;

            // Generate a cryptographically random onboarding token
            employee.OnboardingToken = GenerateSecureToken();
            employee.OnboardingTokenExpiry = DateTime.UtcNow.AddDays(7);

            var createdEmployee = await _employeeRepository.AddAsync(employee);

            // Send invitation email (non-blocking on failure)
            var clientBaseUrl = _configuration["ClientSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
            var onboardingUrl = $"{clientBaseUrl}/onboarding/{createdEmployee.OnboardingToken}";
            await _emailService.SendOnboardingInvitationAsync(
                createdEmployee.Email,
                $"{createdEmployee.FirstName} {createdEmployee.LastName}",
                onboardingUrl);

            _logger.LogInformation($"Employee created with id {createdEmployee.Id}, status Pending, onboarding email sent.");
            return createdEmployee;
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                          .Replace("+", "-")
                          .Replace("/", "_")
                          .Replace("=", "");
        }

        public async Task UpdateEmployeeAsync(int id, Employee employee)
        {
            _logger.LogInformation("Updating employee with id {EmployeeId}.", id);

            if (id != employee.Id)
            {
                _logger.LogInformation("Employee id mismatch. Route id {RouteId}, body id {BodyId}.", id, employee.Id);
                throw new ArgumentException("Employee id mismatch.");
            }

            var existingEmployee = await _employeeRepository.GetByIdAsync(id);
            if (existingEmployee == null)
            {
                _logger.LogInformation($"Failed to update employee. Employee with id {id} not found.");
                throw new KeyNotFoundException($"Employee with id {id} not found.");
            }

            employee.UpdatedAt = DateTime.Now;
            await _employeeRepository.UpdateAsync(employee);

            _logger.LogInformation($"Employee with id {id} updated successfully.");
        }

        public async Task SoftDeleteEmployeeAsync(int id)
        {
            _logger.LogInformation($"Soft deleting (terminating) employee with id {id}.");

            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                _logger.LogInformation($"Cannot delete employee. Employee with id {id} not found.");
                throw new KeyNotFoundException($"Employee with id {id} not found.");
            }

            employee.Status = "Terminated";
            employee.UpdatedAt = DateTime.Now;
            await _employeeRepository.UpdateAsync(employee);

            _logger.LogInformation($"Employee with id {id} terminated successfully.");
        }
    }
}