using HRMS.API.Data;
using HRMS.API.Constants;
using HRMS.API.Models;
using HRMS.API.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IMemoryCache memoryCache,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool Success, string Token, string? ErrorMessage)> RegisterAsync(RegisterModel model)
        {
            var normalizedRole = Roles.Normalize(model.Role);
            if (normalizedRole is null)
            {
                return (false, string.Empty, $"Invalid role. Allowed roles: {string.Join(", ", Roles.All)}.");
            }

            var firstName = model.FirstName?.Trim() ?? string.Empty;
            var lastName = model.LastName?.Trim() ?? string.Empty;
            var phone = model.Phone?.Trim() ?? string.Empty;
            var position = model.Position?.Trim() ?? string.Empty;

            if (!await _departmentRepository.ExistsAsync(model.DepartmentId))
            {
                return (false, string.Empty, "Selected department does not exist.");
            }

            var employeeId = await GenerateUniqueEmployeeIdAsync(model.FirstName);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Role = normalizedRole,
                EmployeeId = employeeId,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, string.Empty, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(normalizedRole));
            }

            await _userManager.AddToRoleAsync(user, normalizedRole);

            try
            {
                var employee = new Employee
                {
                    EmployeeId = employeeId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = model.Email,
                    Phone = phone,
                    DateOfBirth = model.DateOfBirth.Date,
                    DepartmentId = model.DepartmentId,
                    Position = position,
                    Salary = model.Salary,
                    HireDate = DateTime.UtcNow,
                    EmploymentType = string.IsNullOrWhiteSpace(model.EmploymentType)
                        ? "Full-time"
                        : model.EmploymentType.Trim(),
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdEmployee = await _employeeRepository.AddAsync(employee);
                var token = GenerateJwtToken(user, createdEmployee.Id);
                return (true, token, null);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                return (false, string.Empty, "Unable to create employee profile for this account.");
            }
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

        public async Task<(bool Success, string Token, ApplicationUser? User, Employee? Employee, string? ErrorMessage)> LoginAsync(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                return (false, string.Empty, null, null, "Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return (false, string.Empty, null, null, "Invalid credentials");
            }

            Employee? employee = null;
            if (!string.IsNullOrEmpty(user.EmployeeId))
            {
                employee = await _employeeRepository.GetByEmployeeIdAsync(user.EmployeeId);
            }

            var token = GenerateJwtToken(user, employee?.Id);
            return (true, token, user, employee, null);
        }

        public async Task<(ApplicationUser? User, Employee? Employee)> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (null, null);
            }

            Employee? employee = null;
            if (!string.IsNullOrEmpty(user.EmployeeId))
            {
                employee = await _employeeRepository.GetByEmployeeIdAsync(user.EmployeeId);
            }

            return (user, employee);
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string userId, ChangePasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, "User account not found.");
            }

            var result = await _userManager.ChangePasswordAsync(
                user,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return (true, null);
        }

        public async Task RequestPasswordResetOtpAsync(RequestPasswordResetOtpModel model)
        {
            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null || !user.IsActive || string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var otpCode = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var expiryMinutes = 10;

            _memoryCache.Set(
                GetPasswordResetOtpCacheKey(normalizedEmail),
                new PasswordResetOtpCacheEntry(otpCode, DateTime.UtcNow.AddMinutes(expiryMinutes)),
                TimeSpan.FromMinutes(expiryMinutes));

            var recipientName = string.Join(" ", new[] { user.FirstName, user.LastName }
                .Where(x => !string.IsNullOrWhiteSpace(x)))
                .Trim();

            if (string.IsNullOrWhiteSpace(recipientName))
            {
                recipientName = user.Email;
            }

            await _emailService.SendPasswordResetOtpAsync(user.Email, recipientName, otpCode, expiryMinutes);
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordWithOtpAsync(ResetPasswordWithOtpModel model)
        {
            if (!string.Equals(model.NewPassword, model.ConfirmNewPassword, StringComparison.Ordinal))
            {
                return (false, "Confirm new password must match new password.");
            }

            var normalizedEmail = model.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user == null || !user.IsActive)
            {
                return (false, "OTP is invalid or expired.");
            }

            if (!_memoryCache.TryGetValue(GetPasswordResetOtpCacheKey(normalizedEmail), out PasswordResetOtpCacheEntry? otpEntry)
                || otpEntry == null
                || otpEntry.ExpiresAtUtc < DateTime.UtcNow)
            {
                return (false, "OTP is invalid or expired.");
            }

            if (!string.Equals(otpEntry.Code, model.Otp.Trim(), StringComparison.Ordinal))
            {
                return (false, "OTP is invalid or expired.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (!resetResult.Succeeded)
            {
                return (false, string.Join(", ", resetResult.Errors.Select(e => e.Description)));
            }

            _memoryCache.Remove(GetPasswordResetOtpCacheKey(normalizedEmail));
            _logger.LogInformation($"Password reset completed via OTP for {normalizedEmail}");

            return (true, null);
        }

        private static string GetPasswordResetOtpCacheKey(string normalizedEmail) =>
            $"auth:password-reset-otp:{normalizedEmail}";

        public string GenerateJwtToken(ApplicationUser user, int? numericEmployeeId = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey is required.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? Roles.Employee),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (numericEmployeeId.HasValue)
                claims.Add(new Claim("EmployeeId", numericEmployeeId.Value.ToString()));

            var expiryMinutes = Convert.ToDouble(jwtSettings["ExpirationInMinutes"] ?? "20");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DepartmentId { get; set; }
        public string? Position { get; set; }
        public decimal Salary { get; set; }
        public string? EmploymentType { get; set; }
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class RequestPasswordResetOtpModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordWithOtpModel
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public record PasswordResetOtpCacheEntry(string Code, DateTime ExpiresAtUtc);
}