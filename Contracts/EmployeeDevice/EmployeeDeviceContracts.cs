using System.ComponentModel.DataAnnotations;

namespace HRMS.API.Contracts.EmployeeDevice
{
    public class RegisterDeviceRequest
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? DeviceType { get; set; }
    }
}
