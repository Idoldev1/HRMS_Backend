using FluentValidation;
using HRMS.API.Contracts.Leave;

namespace HRMS.API.Validations
{
    public class RejectLeaveRequestValidator : AbstractValidator<RejectLeaveRequest>
    {
        public RejectLeaveRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MaximumLength(500).WithMessage("Rejection reason must not exceed 500 characters.");
        }
    }
}
