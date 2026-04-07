using FluentValidation;
using HRMS.API.Contracts.Leave;

namespace HRMS.API.Validations
{
    public class ApproveLeaveRequestValidator : AbstractValidator<ApproveLeaveRequest>
    {
        public ApproveLeaveRequestValidator()
        {
            RuleFor(x => x.ApprovedById)
                .GreaterThan(0).WithMessage("A valid approver employee ID is required.");
        }
    }
}
