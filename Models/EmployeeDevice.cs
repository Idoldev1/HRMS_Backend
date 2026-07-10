using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.API.Models
{
    public class EmployeeDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [Required]
        [StringLength(255)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? DeviceType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }
    }
}
