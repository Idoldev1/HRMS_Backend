using HRMS.API.Models;

namespace HRMS.API.Repositories
{
    public interface IOnboardingRepository
    {
        /// <summary>
        /// Get a pending employee by their valid (non-expired) onboarding token.
        /// </summary>
        Task<Employee?> GetByTokenAsync(string token);

        /// <summary>
        /// Update employee personal details during onboarding.
        /// </summary>
        Task UpdateEmployeeDetailsAsync(Employee employee);

        /// <summary>
        /// Mark onboarding as complete and activate the employee.
        /// </summary>
        Task CompleteOnboardingAsync(int employeeId);

        /// <summary>
        /// Add an uploaded document to the employee's onboarding.
        /// </summary>
        Task<EmployeeDocument> AddDocumentAsync(EmployeeDocument document);

        /// <summary>
        /// Get all documents for an employee.
        /// </summary>
        Task<IEnumerable<EmployeeDocument>> GetDocumentsByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// Get a specific document by ID and verify it belongs to the employee.
        /// </summary>
        Task<EmployeeDocument?> GetDocumentByIdAsync(int documentId, int employeeId);

        /// <summary>
        /// Remove a document from the database.
        /// </summary>
        Task DeleteDocumentAsync(EmployeeDocument document);
    }
}
