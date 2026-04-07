using FluentValidation;
using HRMS.API.Contracts.Onboarding;

namespace HRMS.API.Validations
{
    public class CreateOnboardingUserProfileRequestValidator : AbstractValidator<CreateOnboardingUserProfileRequest>
    {
        public CreateOnboardingUserProfileRequestValidator()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters.")
                .Matches("[A-Z]").WithMessage("New password must include at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must include at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must include at least one number.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm new password is required.")
                .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}
