using FluentValidation;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class AttendanceValidator : AbstractValidator<Attendance>
    {
        private static readonly string[] AllowedStatuses =
            { "Present", "Absent", "Late", "Half-Day", "Remote", "Signed Out" };

        public AttendanceValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Attendance date is required.")
                .LessThanOrEqualTo(_ => DateTime.Today)
                .WithMessage("Attendance date cannot be in the future.");

            RuleFor(x => x.CheckIn)
                .NotEmpty().WithMessage("Check-in time is required.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .MaximumLength(20).WithMessage("Status must not exceed 20 characters.")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }
    }
}
