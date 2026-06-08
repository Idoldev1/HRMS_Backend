using HRMS.API.Contracts.Onboarding;
using HRMS.API.DTOs;
using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRMS.API.Services
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IOnboardingRepository _repository;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<OnboardingService> _logger;
        private readonly long _maxFileSizeBytes;
        private readonly HashSet<string> _allowedContentTypes;

        public OnboardingService(
            IOnboardingRepository repository,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<OnboardingService> logger,
            IConfiguration configuration)
        {
            _repository = repository;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;

            _maxFileSizeBytes = configuration.GetValue<long>("UploadSettings:MaxFileSizeBytes", 10 * 1024 * 1024);

            var types = configuration["UploadSettings:AllowedContentTypes"]
                ?? "application/pdf,image/jpeg,image/png,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            _allowedContentTypes = new HashSet<string>(
                types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<OnboardingProfileDto?> GetProfileAsync(string token)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("GetProfile: no employee found for onboarding token");
                return null;
            }

            return MapToDto(employee);
        }

        public async Task<OnboardingProfileDto> UpdateProfileAsync(string token, UpdateOnboardingRequest request)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("UpdateProfile: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            // Apply only editable fields
            employee.FirstName = request.FirstName.Trim();
            employee.LastName = request.LastName.Trim();
            employee.Phone = request.Phone.Trim();
            employee.DateOfBirth = request.DateOfBirth;
            employee.Street = request.Street?.Trim();
            employee.City = request.City?.Trim();
            employee.State = request.State?.Trim();
            employee.ZipCode = request.ZipCode?.Trim();
            employee.Country = request.Country?.Trim();
            employee.EmergencyContactName = request.EmergencyContactName?.Trim();
            employee.EmergencyContactRelationship = request.EmergencyContactRelationship?.Trim();
            employee.EmergencyContactPhone = request.EmergencyContactPhone?.Trim();
            employee.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateEmployeeDetailsAsync(employee);

            _logger.LogInformation("Employee {EmployeeId} updated personal details via onboarding", employee.EmployeeId);
            return MapToDto(employee);
        }

        public async Task CompleteAsync(string token)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("CompleteOnboarding: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            await _repository.CompleteOnboardingAsync(employee.Id);
            _logger.LogInformation("Onboarding completed for EmployeeId={EmployeeId}", employee.EmployeeId);
        }

        public async Task<OnboardingDocumentDto> UploadDocumentAsync(string token, string documentType, IFormFile file)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("UploadDocument: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            ValidateFile(file);

            var originalFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";

            var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "employee-documents", employee.Id.ToString());
            Directory.CreateDirectory(uploadsRoot);

            var filePath = Path.Combine(uploadsRoot, storedFileName);
            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var doc = new EmployeeDocument
            {
                EmployeeId = employee.Id,
                DocumentType = documentType.Trim(),
                FileName = originalFileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            await _repository.AddDocumentAsync(doc);

            _logger.LogInformation("Employee {EmployeeId} uploaded document {DocumentType} ({FileName})", employee.Id, doc.DocumentType, doc.FileName);

            return MapDocumentToDto(doc);
        }

        public async Task<IEnumerable<OnboardingDocumentDto>> GetDocumentsAsync(string token)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("GetDocuments: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            var docs = await _repository.GetDocumentsByEmployeeIdAsync(employee.Id);
            return docs.Select(MapDocumentToDto);
        }

        public async Task DeleteDocumentAsync(string token, int docId)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("DeleteDocument: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            var doc = await _repository.GetDocumentByIdAsync(docId, employee.Id);
            if (doc == null)
            {
                _logger.LogWarning("DeleteDocument: document {DocId} not found for EmployeeId={EmployeeId}", docId, employee.Id);
                throw new KeyNotFoundException("Document not found.");
            }

            // Delete physical file
            var filePath = Path.Combine(
                _env.ContentRootPath, "uploads", "employee-documents",
                employee.Id.ToString(), doc.StoredFileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            await _repository.DeleteDocumentAsync(doc);

            _logger.LogInformation("Employee {EmployeeId} deleted document {DocId}", employee.Id, docId);
        }

        public async Task CreateUserProfileAsync(string token, CreateOnboardingUserProfileRequest request)
        {
            var employee = await _repository.GetByTokenAsync(token);
            if (employee == null)
            {
                _logger.LogWarning("CreateUserProfile: invalid or expired onboarding token");
                throw new UnauthorizedAccessException("Invalid or expired onboarding link.");
            }

            var existingByEmail = await _userManager.FindByEmailAsync(employee.Email);
            if (existingByEmail != null)
            {
                _logger.LogWarning("CreateUserProfile: user account already exists for {Email}", employee.Email);
                throw new InvalidOperationException("A user account already exists for this email.");
            }

            var existingByEmployeeId = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == employee.EmployeeId);
            if (existingByEmployeeId != null)
            {
                _logger.LogWarning("CreateUserProfile: user account already exists for EmployeeId={EmployeeId}", employee.EmployeeId);
                throw new InvalidOperationException("A user account already exists for this employee.");
            }

            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmployeeId = employee.EmployeeId,
                Role = "Employee",
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Phone = employee.Phone,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, request.NewPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogWarning("CreateUserProfile: user creation failed for EmployeeId={EmployeeId}: {Errors}", employee.EmployeeId, errors);
                throw new ArgumentException(errors);
            }

            if (await _roleManager.RoleExistsAsync("Employee"))
            {
                await _userManager.AddToRoleAsync(user, "Employee");
            }

            await _repository.CompleteOnboardingAsync(employee.Id);
            _logger.LogInformation("User profile created and onboarding completed for EmployeeId={EmployeeId}", employee.EmployeeId);
        }

        // ── Private Helpers ──────────────────────────────────────────

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided.");

            if (file.Length > _maxFileSizeBytes)
            {
                var limitMb = _maxFileSizeBytes / (1024 * 1024);
                throw new ArgumentException($"File exceeds the {limitMb} MB size limit.");
            }

            if (!_allowedContentTypes.Contains(file.ContentType))
                throw new ArgumentException("File type not allowed. Accepted: PDF, JPEG, PNG, DOC, DOCX.");
        }

        private static OnboardingProfileDto MapToDto(Employee e) => new()
        {
            Id = e.Id,
            EmployeeId = e.EmployeeId,
            Email = e.Email,
            DepartmentName = e.Department?.Name ?? string.Empty,
            Position = e.Position,
            Salary = e.Salary,
            HireDate = e.HireDate,
            EmploymentType = e.EmploymentType,
            Status = e.Status,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Phone = e.Phone,
            DateOfBirth = e.DateOfBirth,
            Street = e.Street,
            City = e.City,
            State = e.State,
            ZipCode = e.ZipCode,
            Country = e.Country,
            EmergencyContactName = e.EmergencyContactName,
            EmergencyContactRelationship = e.EmergencyContactRelationship,
            EmergencyContactPhone = e.EmergencyContactPhone,
            Documents = (e.Documents ?? []).Select(MapDocumentToDto).ToList()
        };

        private static OnboardingDocumentDto MapDocumentToDto(EmployeeDocument d) => new()
        {
            Id = d.Id,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FileSizeBytes = d.FileSizeBytes,
            UploadedAt = d.UploadedAt
        };
    }
}
