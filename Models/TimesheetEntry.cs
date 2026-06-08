using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class TimesheetEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TimesheetId { get; set; }

        [ForeignKey("TimesheetId")]
        public Timesheet? Timesheet { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [StringLength(20)]
        public string? AttendanceStatus { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursWorked { get; set; }

        public string? TasksCompleted { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
