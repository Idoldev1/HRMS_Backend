using System.Text.Json.Serialization;

namespace HRMS.API.Services
{
    public class BrevoSettings
    {
        public string ApiUrl { get; set; } = "https://api.brevo.com/v3/smtp/email";
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "HRMS System";
    }

    public class EmailService : IEmailService
    {
        private readonly BrevoSettings _brevo;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, HttpClient httpClient, ILogger<EmailService> logger)
        {
            _brevo = configuration.GetSection("BrevoSettings").Get<BrevoSettings>()
                ?? new BrevoSettings();
            _httpClient = httpClient;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization = null;
            _httpClient.DefaultRequestHeaders.Remove("api-key");

            if (!string.IsNullOrWhiteSpace(_brevo.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", _brevo.ApiKey);
            }
        }

        public async Task SendOnboardingInvitationAsync(string toEmail, string employeeName, string onboardingUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_brevo.ApiKey))
                {
                  _logger.LogInformation($"Brevo API key is not configured. Skipping onboarding email to {toEmail}.");
                    return;
                }

                var payload = new BrevoSendEmailRequest
                {
                    Sender = new BrevoEmailAddress
                    {
                        Email = _brevo.FromEmail,
                        Name = _brevo.FromName
                    },
                    To = new[]
                    {
                        new BrevoEmailAddress
                        {
                            Email = toEmail,
                            Name = employeeName
                        }
                    },
                    Subject = "Welcome to HRMS – Complete Your Onboarding",
                    HtmlContent = BuildOnboardingEmailHtml(employeeName, onboardingUrl),
                    TextContent = $"Hello {employeeName},\n\nYou have been added to HRMS. " +
                        $"Please complete your onboarding by visiting:\n{onboardingUrl}\n\n" +
                        "This link expires in 7 days.\n\nRegards,\nHR Team"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_brevo.ApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Onboarding invitation sent to {toEmail} via Brevo.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                  _logger.LogInformation($"Brevo API error ({response.StatusCode}): {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send onboarding email to {toEmail}.");
                // Do not rethrow – email failure should not block employee creation
            }
        }

        public async Task SendPasswordResetOtpAsync(string toEmail, string recipientName, string otpCode, int expiryMinutes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_brevo.ApiKey))
                {
                    _logger.LogInformation("Brevo API key is not configured. Skipping password reset OTP email to {ToEmail}.", toEmail);
                    return;
                }

                var payload = new BrevoSendEmailRequest
                {
                    Sender = new BrevoEmailAddress
                    {
                        Email = _brevo.FromEmail,
                        Name = _brevo.FromName
                    },
                    To = new[]
                    {
                        new BrevoEmailAddress
                        {
                            Email = toEmail,
                            Name = recipientName
                        }
                    },
                    Subject = "HRMS Password Reset Code",
                    HtmlContent = BuildPasswordResetOtpEmailHtml(recipientName, otpCode, expiryMinutes),
                    TextContent =
                        $"Hello {recipientName},\n\n" +
                        $"Your HRMS password reset code is: {otpCode}\n" +
                        $"This code expires in {expiryMinutes} minutes.\n\n" +
                        "If you did not request this change, please ignore this email."
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_brevo.ApiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Password reset OTP sent to {toEmail} via Brevo.");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Brevo API error while sending OTP ({response.StatusCode}): {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset OTP email to {toEmail}.");
            }
        }

        private static string BuildOnboardingEmailHtml(string name, string url) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><title>HRMS Onboarding</title></head>
            <body style="margin:0;padding:0;font-family:'Segoe UI',Arial,sans-serif;background:#f4f6fb;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6fb;padding:40px 0;">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08);">
                    <tr><td style="background:#1976d2;padding:32px 40px;">
                      <h1 style="margin:0;color:#fff;font-size:24px;font-weight:700;">Welcome to HRMS</h1>
                    </td></tr>
                    <tr><td style="padding:40px;">
                      <p style="font-size:16px;color:#333;margin:0 0 16px;">Hello <strong>{name}</strong>,</p>
                      <p style="font-size:15px;color:#555;margin:0 0 24px;">
                        You have been registered in our HR Management System. To complete your onboarding,
                        please click the button below to review your information and upload the required documents.
                      </p>
                      <p style="text-align:center;margin:0 0 24px;">
                        <a href="{url}" style="display:inline-block;background:#1976d2;color:#fff;text-decoration:none;padding:14px 32px;border-radius:6px;font-size:15px;font-weight:600;">
                          Complete Onboarding
                        </a>
                      </p>
                      <p style="font-size:13px;color:#888;margin:0 0 8px;">Or copy and paste this link into your browser:</p>
                      <p style="font-size:13px;color:#1976d2;word-break:break-all;margin:0 0 24px;">{url}</p>
                      <p style="font-size:13px;color:#e57373;margin:0;">⚠ This link expires in <strong>7 days</strong>. Please complete your onboarding before then.</p>
                    </td></tr>
                    <tr><td style="background:#f4f6fb;padding:20px 40px;text-align:center;">
                      <p style="font-size:12px;color:#aaa;margin:0;">HRMS &bull; Do not reply to this email</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        private static string BuildPasswordResetOtpEmailHtml(string name, string otpCode, int expiryMinutes) => $"""
            <!DOCTYPE html>
            <html lang="en">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><title>Password Reset</title></head>
            <body style="margin:0;padding:0;font-family:'Segoe UI',Arial,sans-serif;background:#f4f6fb;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6fb;padding:40px 0;">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08);">
                    <tr><td style="background:#1f2937;padding:28px 40px;">
                      <h1 style="margin:0;color:#fff;font-size:22px;font-weight:700;">Password Reset Verification</h1>
                    </td></tr>
                    <tr><td style="padding:32px 40px;">
                      <p style="font-size:16px;color:#333;margin:0 0 14px;">Hello <strong>{name}</strong>,</p>
                      <p style="font-size:15px;color:#555;margin:0 0 20px;">Use the code below to reset your HRMS password:</p>
                      <p style="text-align:center;margin:0 0 20px;">
                        <span style="display:inline-block;font-size:30px;letter-spacing:8px;font-weight:700;color:#111827;background:#eef2ff;border:1px solid #c7d2fe;padding:12px 18px;border-radius:6px;">{otpCode}</span>
                      </p>
                      <p style="font-size:14px;color:#6b7280;margin:0;">This code expires in <strong>{expiryMinutes} minutes</strong>.</p>
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        private class BrevoSendEmailRequest
        {
            [JsonPropertyName("sender")]
            public BrevoEmailAddress Sender { get; set; } = new();

            [JsonPropertyName("to")]
            public BrevoEmailAddress[] To { get; set; } = Array.Empty<BrevoEmailAddress>();

            [JsonPropertyName("subject")]
            public string Subject { get; set; } = string.Empty;

            [JsonPropertyName("htmlContent")]
            public string HtmlContent { get; set; } = string.Empty;

            [JsonPropertyName("textContent")]
            public string TextContent { get; set; } = string.Empty;
        }

        private class BrevoEmailAddress
        {
            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }
    }
}
