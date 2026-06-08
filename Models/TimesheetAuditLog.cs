using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class TimesheetAuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TimesheetId { get; set; }

        [ForeignKey("TimesheetId")]
        public Timesheet? Timesheet { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(450)]
        public string PerformedById { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PerformedByName { get; set; } = string.Empty;

        public string? Comment { get; set; }

        [StringLength(20)]
        public string? PreviousStatus { get; set; }

        [StringLength(20)]
        public string? NewStatus { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.Now;
    }
}
