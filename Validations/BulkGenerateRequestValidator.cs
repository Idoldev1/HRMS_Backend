using FluentValidation;
using HRMS.API.Controllers;

namespace HRMS.API.Validations
{
    public class BulkGenerateRequestValidator : AbstractValidator<BulkGenerateRequest>
    {
        public BulkGenerateRequestValidator()
        {
            RuleFor(x => x.EmployeeIds)
                .NotNull().WithMessage("Employee IDs are required.")
                .Must(ids => ids != null && ids.Any())
                .WithMessage("At least one employee ID is required.");

            RuleFor(x => x.PeriodStart)
                .NotEmpty().WithMessage("Period start date is required.");

            RuleFor(x => x.PeriodEnd)
                .NotEmpty().WithMessage("Period end date is required.")
                .GreaterThanOrEqualTo(x => x.PeriodStart)
                .WithMessage("Period end date must be on or after the start date.");
        }
    }
}
