namespace HRMS.API.Contracts.Recruitment
{
    public class CreateJobPostingRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string EmploymentType { get; set; }
        public string WorkMode { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? ClosingDate { get; set; }
    }

    public class UpdateJobPostingRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = "Full-Time";
        public string WorkMode { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? ClosingDate { get; set; }
    }

    public class UpdateJobStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateApplicationStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}
