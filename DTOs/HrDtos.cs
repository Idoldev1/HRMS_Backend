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
        public double OverallRating { get; set; }
        public double? QualityRating { get; set; }
        public double? ProductivityRating { get; set; }
        public double? CommunicationRating { get; set; }
        public double? TeamworkRating { get; set; }
        public double? LeadershipRating { get; set; }
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

    public class JobPostingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string WorkMode { get; set; } = string.Empty;
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ClosingDate { get; set; }
        public int? PostedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ApplicationCount { get; set; }
    }

    public class JobPostingDetailDto : JobPostingDto
    {
        public List<JobApplicationDto> Applications { get; set; } = new();
    }

    public class JobApplicationDto
    {
        public int Id { get; set; }
        public int JobPostingId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string? CandidatePhone { get; set; }
        public string? CvFilePath { get; set; }
        public string? CoverLetter { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? JobTitle { get; set; }
    }
}
