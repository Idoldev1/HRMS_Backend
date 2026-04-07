namespace HRMS.API.DTOs
{
    public class AuthUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
    }

    public class AuthEmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }

    public class AuthSessionDto
    {
        public string Token { get; set; } = string.Empty;
        public AuthUserDto User { get; set; } = new();
        public AuthEmployeeDto? Employee { get; set; }
    }

    public class AuthTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
