using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CheckIn { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CheckOut { get; set; }

        public int BreakDuration { get; set; } = 0; // in minutes

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalHours { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Present";

        public string? Notes { get; set; }

        [StringLength(20)]
        public string Location { get; set; } = "Office";

        [StringLength(255)]
        public string? DeviceId { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? CheckInLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? CheckInLongitude { get; set; }

        public int? WorkLocationId { get; set; }

        [ForeignKey("WorkLocationId")]
        public WorkLocation? WorkLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
