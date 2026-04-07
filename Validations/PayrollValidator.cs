using FluentValidation;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class PayrollValidator : AbstractValidator<Payroll>
    {
        private static readonly string[] AllowedStatuses =
            { "Pending", "Processing", "Processed", "Paid" };

        public PayrollValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.PayPeriodStartDate)
                .NotEmpty().WithMessage("Pay period start date is required.");

            RuleFor(x => x.PayPeriodEndDate)
                .NotEmpty().WithMessage("Pay period end date is required.")
                .GreaterThanOrEqualTo(x => x.PayPeriodStartDate)
                .WithMessage("Pay period end date must be on or after the start date.");

            RuleFor(x => x.BaseSalary)
                .GreaterThan(0).WithMessage("Base salary must be greater than zero.");

            RuleFor(x => x.TotalEarnings)
                .GreaterThanOrEqualTo(0).WithMessage("Total earnings must be zero or greater.");

            RuleFor(x => x.TotalDeductions)
                .GreaterThanOrEqualTo(0).WithMessage("Total deductions must be zero or greater.");

            RuleFor(x => x.NetSalary)
                .GreaterThanOrEqualTo(0).WithMessage("Net salary must be zero or greater.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }
    }
}
