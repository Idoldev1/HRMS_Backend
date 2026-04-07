namespace HRMS.API.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string HR = "HR";
        public const string Manager = "Manager";
        public const string Employee = "Employee";
        public const string Finance = "Finance";

        public static readonly string[] All =
        {
            Admin,
            HR,
            Manager,
            Employee,
            Finance
        };

        public static string? Normalize(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            return All.FirstOrDefault(r =>
                string.Equals(r, role.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsValid(string? role) => Normalize(role) is not null;
    }
}