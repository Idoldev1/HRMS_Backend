using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class Payslip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PayrollId { get; set; }

        [ForeignKey("PayrollId")]
        public Payroll? Payroll { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? DownloadedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string PayslipNumber { get; set; } = string.Empty;

        [Required]
        public string PayslipContent { get; set; } = string.Empty;  // HTML content

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
