using HRMS.API.Constants;
using HRMS.API.Contracts.CompanyRegistration;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.AspNetCore.Identity;

namespace HRMS.API.Services
{
    public class CompanyRegistrationService : ICompanyRegistrationService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IOnboardingRepository _onboardingRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<CompanyRegistrationService> _logger;
        private readonly long _maxFileSizeBytes;
        private readonly HashSet<string> _allowedContentTypes;

        public CompanyRegistrationService(
            ICompanyRepository companyRepository,
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IOnboardingRepository onboardingRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuthService authService,
            IWebHostEnvironment env,
            ILogger<CompanyRegistrationService> logger,
            IConfiguration configuration)
        {
            _companyRepository = companyRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _onboardingRepository = onboardingRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
            _env = env;
            _logger = logger;

            _maxFileSizeBytes = configuration.GetValue<long>("UploadSettings:MaxFileSizeBytes", 10 * 1024 * 1024);

            var types = configuration["UploadSettings:AllowedContentTypes"]
                ?? "application/pdf,image/jpeg,image/png,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            _allowedContentTypes = new HashSet<string>(
                types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<(bool Success, string Token, CompanyRegistrationResponseDto? Response, string? ErrorMessage)>
            RegisterCompanyAsync(CompanyRegistrationRequest request)
        {
            // Validate department exists
            if (!await _departmentRepository.ExistsAsync(request.DepartmentId))
            {
                return (false, string.Empty, null, "Selected department does not exist.");
            }

            // Check if company email is already taken
            var existingCompany = await _companyRepository.GetByEmailAsync(request.CompanyEmail);
            if (existingCompany != null)
            {
                return (false, string.Empty, null, "A company with this email already exists.");
            }

            // Create the company
            var company = new Company
            {
                Name = request.CompanyName.Trim(),
                RegistrationNumber = request.CompanyRegistrationNumber?.Trim(),
                Industry = request.Industry?.Trim(),
                Email = request.CompanyEmail.Trim(),
                Phone = request.CompanyPhone.Trim(),
                Website = request.CompanyWebsite?.Trim(),
                Street = request.CompanyStreet?.Trim(),
                City = request.CompanyCity?.Trim(),
                State = request.CompanyState?.Trim(),
                ZipCode = request.CompanyZipCode?.Trim(),
                Country = request.CompanyCountry?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdCompany = await _companyRepository.AddAsync(company);

            // Generate unique employee ID
            var employeeId = await GenerateUniqueEmployeeIdAsync(request.FirstName);

            // Create the ApplicationUser (Admin role for company registrant)
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                Role = Roles.Admin,
                EmployeeId = employeeId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone.Trim(),
                IsActive = true
            };

            var identityResult = await _userManager.CreateAsync(user, request.Password);
            if (!identityResult.Succeeded)
            {
                // Rollback company creation
                await _companyRepository.DeleteAsync(createdCompany);
                return (false, string.Empty, null, string.Join(", ", identityResult.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(Roles.Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }
            await _userManager.AddToRoleAsync(user, Roles.Admin);

            try
            {
                // Create the employee record with full personal info
                var employee = new Employee
                {
                    EmployeeId = employeeId,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email,
                    Phone = request.Phone.Trim(),
                    DateOfBirth = request.DateOfBirth.Date,
                    Street = request.Street?.Trim(),
                    City = request.City?.Trim(),
                    State = request.State?.Trim(),
                    ZipCode = request.ZipCode?.Trim(),
                    Country = request.Country?.Trim(),
                    EmergencyContactName = request.EmergencyContactName?.Trim(),
                    EmergencyContactRelationship = request.EmergencyContactRelationship?.Trim(),
                    EmergencyContactPhone = request.EmergencyContactPhone?.Trim(),
                    DepartmentId = request.DepartmentId,
                    Position = request.Position?.Trim() ?? "Admin",
                    Salary = request.Salary,
                    GrossSalary = request.GrossSalary,
                    FederalTaxRate = request.FederalTaxRate,
                    InsuranceRate = request.InsuranceRate,
                    HireDate = DateTime.UtcNow,
                    EmploymentType = string.IsNullOrWhiteSpace(request.EmploymentType)
                        ? "Full-time"
                        : request.EmploymentType.Trim(),
                    Status = "Active",
                    CompanyId = createdCompany.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdEmployee = await _employeeRepository.AddAsync(employee);
                var token = _authService.GenerateJwtToken(user, createdEmployee.EmployeeId);

                var response = new CompanyRegistrationResponseDto
                {
                    Token = token,
                    Message = "Company registration successful.",
                    Company = createdCompany.ToDto(),
                    Employee = new AuthEmployeeDto
                    {
                        Id = createdEmployee.Id,
                        FirstName = createdEmployee.FirstName,
                        LastName = createdEmployee.LastName,
                        Position = createdEmployee.Position,
                    }
                };

                _logger.LogInformation("Company '{CompanyName}' registered by {Email}.", company.Name, request.Email);

                return (true, token, response, null);
            }
            catch
            {
                // Rollback user and company if employee creation fails
                await _userManager.DeleteAsync(user);
                await _companyRepository.DeleteAsync(createdCompany);
                return (false, string.Empty, null, "Unable to complete company registration.");
            }
        }

        public async Task<OnboardingDocumentDto> UploadDocumentAsync(int employeeId, string documentType, IFormFile file)
        {
            ValidateFile(file);

            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "employee-documents", employeeId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var filePath = Path.Combine(uploadsRoot, storedFileName);
            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new EmployeeDocument
            {
                EmployeeId = employeeId,
                DocumentType = documentType.Trim(),
                FileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            // Save via the onboarding repository which handles document persistence
            await _onboardingRepository.AddDocumentAsync(doc);

            _logger.LogInformation("Document '{DocType}' uploaded for employee {EmployeeId}.", documentType, employeeId);

            return new OnboardingDocumentDto
            {
                Id = doc.Id,
                DocumentType = doc.DocumentType,
                FileName = doc.FileName,
                FileSizeBytes = doc.FileSizeBytes,
                UploadedAt = doc.UploadedAt,
            };
        }

        private void ValidateFile(IFormFile file)
        {
            if (file.Length == 0)
                throw new ArgumentException("File is empty.");
            if (file.Length > _maxFileSizeBytes)
                throw new ArgumentException($"File exceeds maximum allowed size of {_maxFileSizeBytes / (1024 * 1024)} MB.");
            if (!_allowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException($"File type '{file.ContentType}' is not allowed.");
        }

        private async Task<string> GenerateUniqueEmployeeIdAsync(string? firstName)
        {
            var lettersOnly = new string((firstName ?? string.Empty).Where(char.IsLetter).ToArray());
            var prefix = string.IsNullOrWhiteSpace(lettersOnly)
                ? "EMP"
                : lettersOnly[..Math.Min(3, lettersOnly.Length)].ToUpperInvariant();

            var random = new Random();
            string candidate;

            do
            {
                candidate = $"{prefix}{random.Next(1000, 9999)}";
            }
            while (await _employeeRepository.GetByEmployeeIdAsync(candidate) != null);

            return candidate;
        }
    }
}
