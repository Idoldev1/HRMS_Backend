using FluentValidation;
using HRMS.API.Contracts.Onboarding;

namespace HRMS.API.Validations
{
    public class UpdateOnboardingRequestValidator : AbstractValidator<UpdateOnboardingRequest>
    {
        public UpdateOnboardingRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob != default && dob.Date < DateTime.UtcNow.Date)
                .WithMessage("Date of birth must be a valid past date.");
        }
    }
}
