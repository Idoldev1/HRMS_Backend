using System.ComponentModel.DataAnnotations;

namespace HRMS.API.Models
{
    public class JobPosting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Location { get; set; }

        [Required]
        [StringLength(50)]
        public string EmploymentType { get; set; } = "Full-Time";

        [Required]
        [StringLength(20)]
        public string WorkMode { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public string? Requirements { get; set; }

        public string? Responsibilities { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        public DateTime? ClosingDate { get; set; }

        public int? PostedById { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    }
}
