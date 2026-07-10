using System.ComponentModel.DataAnnotations;

namespace HRMS.API.Contracts.WorkLocation
{
    public class CreateWorkLocationRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal Longitude { get; set; }

        [Required]
        [Range(10, 50000, ErrorMessage = "Allowed radius must be between 10 and 50000 meters.")]
        public double AllowedRadiusMeters { get; set; } = 100;
    }

    public class UpdateWorkLocationRequest : CreateWorkLocationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class AssignWorkLocationRequest
    {
        [Required]
        public int WorkLocationId { get; set; }
    }
}
