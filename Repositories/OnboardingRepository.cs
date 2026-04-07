using HRMS.API.Data;
using HRMS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Repositories
{
    public class OnboardingRepository : IOnboardingRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<OnboardingRepository> _logger;

        public OnboardingRepository(ApplicationDbContext db, ILogger<OnboardingRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Employee?> GetByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;

            return await _db.Employees
                            .Include(e => e.Department)
                            .Include(e => e.Documents)
                            .FirstOrDefaultAsync(e =>
                                e.OnboardingToken == token &&
                                e.OnboardingTokenExpiry > DateTime.UtcNow &&
                                e.Status == "Pending");
        }

        public async Task UpdateEmployeeDetailsAsync(Employee employee)
        {
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Employee {employee.Id} personal details updated.");
        }

        public async Task CompleteOnboardingAsync(int employeeId)
        {
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

            employee.Status = "Active";
            employee.OnboardingToken = null;
            employee.OnboardingTokenExpiry = null;
            employee.UpdatedAt = DateTime.UtcNow;

            _db.Employees.Update(employee);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Employee {employee.EmployeeId} onboarding completed. Status set to Active.");
        }

        public async Task<EmployeeDocument> AddDocumentAsync(EmployeeDocument document)
        {
            _db.EmployeeDocuments.Add(document);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Document {document.Id} uploaded for employee {document.EmployeeId}.");
            return document;
        }

        public async Task<IEnumerable<EmployeeDocument>> GetDocumentsByEmployeeIdAsync(int employeeId)
        {
            return await _db.EmployeeDocuments
                            .Where(d => d.EmployeeId == employeeId)
                            .OrderByDescending(d => d.UploadedAt)
                            .ToListAsync();
        }

        public async Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId, int employeeId)
        {
            return await _db.EmployeeDocuments
                            .FirstOrDefaultAsync(d => d.Id == documentId && d.EmployeeId == employeeId);
        }

        public async Task DeleteDocumentAsync(EmployeeDocument document)
        {
            _db.EmployeeDocuments.Remove(document);
            await _db.SaveChangesAsync();
            _logger.LogInformation($"Document {document.Id} deleted for employee {document.EmployeeId}.");
        }
    }
}
