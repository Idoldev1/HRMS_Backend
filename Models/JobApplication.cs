using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int JobPostingId { get; set; }

        [ForeignKey("JobPostingId")]
        public JobPosting? JobPosting { get; set; }

        [Required]
        [StringLength(100)]
        public string CandidateName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string CandidateEmail { get; set; } = string.Empty;

        [StringLength(30)]
        public string? CandidatePhone { get; set; }

        public string? CvFilePath { get; set; }

        public string? CoverLetter { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Applied";

        public string? Notes { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
