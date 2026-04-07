namespace HRMS.API.Services
{
    public interface IEmailService
    {
        Task SendOnboardingInvitationAsync(string toEmail, string employeeName, string onboardingUrl);
        Task SendPasswordResetOtpAsync(string toEmail, string recipientName, string otpCode, int expiryMinutes);
    }
}
