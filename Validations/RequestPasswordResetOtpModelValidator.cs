using FluentValidation;
using HRMS.API.Services;

namespace HRMS.API.Validations
{
    public class RequestPasswordResetOtpModelValidator : AbstractValidator<RequestPasswordResetOtpModel>
    {
        public RequestPasswordResetOtpModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }
}
