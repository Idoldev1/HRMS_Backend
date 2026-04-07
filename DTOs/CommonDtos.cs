namespace HRMS.API.DTOs
{
    public record MessageDto(string Message);

    public class DepartmentSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class EmployeeDocumentDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class EmployeeSummaryDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DepartmentSummaryDto? Department { get; set; }
        public string? JobTitle { get; set; }
    }
}
