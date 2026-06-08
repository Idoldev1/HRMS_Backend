using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class PerformanceReview
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReviewPeriodStartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReviewPeriodEndDate { get; set; }

        [Required]
        public int ReviewedById { get; set; }

        [ForeignKey("ReviewedById")]
        public Employee? ReviewedBy { get; set; }

        [Required]
        [Range(0.5, 5)]
        public double OverallRating { get; set; }

        [Range(0.5, 5)]
        public double? QualityRating { get; set; }

        [Range(0.5, 5)]
        public double? ProductivityRating { get; set; }

        [Range(0.5, 5)]
        public double? CommunicationRating { get; set; }

        [Range(0.5, 5)]
        public double? TeamworkRating { get; set; }

        [Range(0.5, 5)]
        public double? LeadershipRating { get; set; }

        public string? Strengths { get; set; }

        public string? AreasForImprovement { get; set; }

        public string? ReviewerComments { get; set; }

        public string? EmployeeComments { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        [DataType(DataType.Date)]
        public DateTime? NextReviewDate { get; set; }

        public string? ApprovalComment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
