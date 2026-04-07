using FluentValidation;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class LeaveValidator : AbstractValidator<Leave>
    {
        private static readonly string[] AllowedLeaveTypes =
            { "Annual", "Sick", "Maternity", "Paternity", "Unpaid", "Other" };

        public LeaveValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.LeaveType)
                .NotEmpty().WithMessage("Leave type is required.")
                .MaximumLength(50).WithMessage("Leave type must not exceed 50 characters.")
                .Must(t => AllowedLeaveTypes.Contains(t))
                .WithMessage($"Leave type must be one of: {string.Join(", ", AllowedLeaveTypes)}.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after the start date.");

            RuleFor(x => x.TotalDays)
                .GreaterThan(0).WithMessage("Total days must be greater than zero.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason for leave is required.");
        }
    }
}
