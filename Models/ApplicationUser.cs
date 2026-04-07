using Microsoft.AspNetCore.Identity;

namespace HRMS.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? EmployeeId { get; set; }
        public string Role { get; set; } = "Employee";
        public bool IsActive { get; set; } = true;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        
        // Navigation property
        public Employee? Employee { get; set; }
    }
}
