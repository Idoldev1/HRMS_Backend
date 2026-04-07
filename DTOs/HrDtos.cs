namespace HRMS.API.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Budget { get; set; }
        public string? Location { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
        public DepartmentSummaryDto? Department { get; set; }
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ManagerId { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public List<EmployeeDocumentDto> Documents { get; set; } = new();
    }

    public class AttendanceDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int BreakDuration { get; set; }
        public decimal TotalHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Location { get; set; } = string.Empty;
        public EmployeeSummaryDto? Employee { get; set; }
    }

    public class LeaveDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string LeaveType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public EmployeeSummaryDto? Employee { get; set; }
        public EmployeeSummaryDto? ApprovedBy { get; set; }
    }

    public class PerformanceReviewDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ReviewedById { get; set; }
        public DateTime ReviewPeriodStartDate { get; set; }
        public DateTime ReviewPeriodEndDate { get; set; }
        public int OverallRating { get; set; }
        public int? QualityRating { get; set; }
        public int? ProductivityRating { get; set; }
        public int? CommunicationRating { get; set; }
        public int? TeamworkRating { get; set; }
        public int? LeadershipRating { get; set; }
        public string? Strengths { get; set; }
        public string? AreasForImprovement { get; set; }
        public string? ReviewerComments { get; set; }
        public string? EmployeeComments { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? NextReviewDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public EmployeeSummaryDto? Employee { get; set; }
    }
}
