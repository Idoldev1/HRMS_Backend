namespace HRMS.API.Validations
{
    public static class RecruitmentInputSanitizer
    {
        public static string NormalizeName(string? name) => (name ?? string.Empty).Trim();

        public static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

        public static string NormalizeEmailForStorage(string? email) => (email ?? string.Empty).Trim();
    }
}
