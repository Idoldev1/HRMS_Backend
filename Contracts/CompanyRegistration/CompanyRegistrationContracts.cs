namespace HRMS.API.Contracts.CompanyRegistration
{
    /// <summary>
    /// Request model for company registration at signup.
    /// Accepts company details, personal/employee info, and documents.
    /// </summary>
    public class CompanyRegistrationRequest
    {
        // ── Account credentials ──
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        // ── Company details ──
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyRegistrationNumber { get; set; }
        public string? Industry { get; set; }
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string? CompanyWebsite { get; set; }
        public string? CompanyStreet { get; set; }
        public string? CompanyCity { get; set; }
        public string? CompanyState { get; set; }
        public string? CompanyZipCode { get; set; }
        public string? CompanyCountry { get; set; }

        // ── Personal information ──
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Country { get; set; }

        // ── Emergency contact ──
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactRelationship { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // ── Employment info ──
        public string? Position { get; set; }
        public int DepartmentId { get; set; }
        public decimal Salary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal FederalTaxRate { get; set; }
        public decimal InsuranceRate { get; set; }
        public string? EmploymentType { get; set; }
    }
}
