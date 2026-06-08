using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class Timesheet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public int TotalWorkingDays { get; set; }

        public int PresentDays { get; set; }

        public int LateDays { get; set; }

        public int OnLeaveDays { get; set; }

        public int AbsentDays { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal TotalHours { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Draft";

        public int? ApprovedById { get; set; }

        [ForeignKey("ApprovedById")]
        public Employee? ApprovedBy { get; set; }

        public string? ApprovalComment { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public bool IsLocked { get; set; } = false;

        public DateTime? LockedAt { get; set; }

        [StringLength(255)]
        public string? LockedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<TimesheetEntry>? Entries { get; set; }

        public ICollection<TimesheetAuditLog>? AuditLogs { get; set; }
    }
}
