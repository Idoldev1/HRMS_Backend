using FluentValidation;
using HRMS.API.Contracts.CompanyRegistration;

namespace HRMS.API.Validations
{
    public class CompanyRegistrationRequestValidator : AbstractValidator<CompanyRegistrationRequest>
    {
        public CompanyRegistrationRequestValidator()
        {
            // ── Account credentials ──
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(255);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            // ── Company details ──
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required.")
                .MaximumLength(200);

            RuleFor(x => x.CompanyEmail)
                .NotEmpty().WithMessage("Company email is required.")
                .EmailAddress().WithMessage("A valid company email address is required.")
                .MaximumLength(255);

            RuleFor(x => x.CompanyPhone)
                .NotEmpty().WithMessage("Company phone is required.")
                .MaximumLength(20);

            RuleFor(x => x.Industry)
                .MaximumLength(100);

            RuleFor(x => x.CompanyRegistrationNumber)
                .MaximumLength(100);

            RuleFor(x => x.CompanyWebsite)
                .MaximumLength(255);

            // ── Personal information ──
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob != default && dob.Date < DateTime.UtcNow.Date)
                .WithMessage("Date of birth must be a valid past date.");

            // ── Employment info ──
            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("A valid department is required.");

            RuleFor(x => x.Position)
                .MaximumLength(100);

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0).WithMessage("Salary must be zero or greater.");

            RuleFor(x => x.GrossSalary)
                .GreaterThanOrEqualTo(0).WithMessage("Gross salary must be zero or greater.");

            RuleFor(x => x.FederalTaxRate)
                .InclusiveBetween(0, 1).WithMessage("Federal tax rate must be between 0 and 1.");

            RuleFor(x => x.InsuranceRate)
                .InclusiveBetween(0, 1).WithMessage("Insurance rate must be between 0 and 1.");
        }
    }
}
