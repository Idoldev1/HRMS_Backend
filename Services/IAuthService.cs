using HRMS.API.Models;

namespace HRMS.API.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string? ErrorMessage)> RegisterAsync(RegisterModel model);
        Task<(bool Success, string Token, ApplicationUser? User, Employee? Employee, string? ErrorMessage)> LoginAsync(LoginModel model);
        Task<(ApplicationUser? User, Employee? Employee)> GetCurrentUserAsync(string userId);
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string userId, ChangePasswordModel model);
        Task RequestPasswordResetOtpAsync(RequestPasswordResetOtpModel model);
        Task<(bool Success, string? ErrorMessage)> ResetPasswordWithOtpAsync(ResetPasswordWithOtpModel model);
        string GenerateJwtToken(ApplicationUser user, int? numericEmployeeId = null);
    }
}