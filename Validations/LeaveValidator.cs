using FluentValidation;
using HRMS.API.Contracts.Leave;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class LeaveValidator : AbstractValidator<Leave>
    {
        private static readonly string[] AllowedLeaveTypes =
            { "Annual", "Annual Leave", "Sick", "Sick Leave", "Personal", "Personal Leave", "Maternity", "Paternity", "Emergency", "Unpaid", "Other" };

        public LeaveValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.LeaveType)
                .NotEmpty().WithMessage("Leave type is required.")
                .MaximumLength(50).WithMessage("Leave type must not exceed 50 characters.")
                .Must(t => AllowedLeaveTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Leave type must be one of: {string.Join(", ", AllowedLeaveTypes)}.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .GreaterThan(DateTime.Today)
                .WithMessage("Start date must be a future date.");

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

    public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequest>
    {
        private const int MaxLeaveDays = 10;
        private static readonly string[] AllowedLeaveTypes =
            { "Annual", "Annual Leave", "Sick", "Sick Leave", "Personal", "Personal Leave", "Maternity", "Paternity", "Emergency", "Unpaid", "Other" };

        public CreateLeaveRequestValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.LeaveType)
                .NotEmpty().WithMessage("Leave type is required.")
                .MaximumLength(50).WithMessage("Leave type must not exceed 50 characters.")
                .Must(t => AllowedLeaveTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Leave type must be one of: {string.Join(", ", AllowedLeaveTypes)}.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required.")
                .GreaterThan(DateTime.Today)
                .WithMessage("Start date must be a future date.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after the start date.");

            RuleFor(x => x)
                .Must(x => ((x.EndDate.Date - x.StartDate.Date).Days + 1) <= MaxLeaveDays)
                .WithMessage($"Leave request cannot exceed {MaxLeaveDays} days.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason for leave is required.");
        }
    }
}
