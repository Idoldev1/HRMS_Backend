using System.ComponentModel.DataAnnotations;

namespace HRMS.API.Contracts.Attendance
{
    public class MarkAttendanceRequest
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal Longitude { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "DeviceId is required.")]
        public string DeviceId { get; set; } = string.Empty;
    }

    public class CheckOutRequest
    {
        [Range(0, 480, ErrorMessage = "Break duration must be between 0 and 480 minutes.")]
        public int BreakDuration { get; set; } = 0;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
