using FluentValidation;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class EmployeeValidator : AbstractValidator<Employee>
    {
        private static readonly string[] AllowedEmploymentTypes =
            { "Full-time", "Part-time", "Contract", "Intern" };

        public EmployeeValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.");

            RuleFor(x => x.DateOfBirth)
                .Must(dob => dob != default && dob.Date < DateTime.UtcNow.Date)
                .WithMessage("Date of birth must be a valid past date.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("A valid department is required.");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Position is required.")
                .MaximumLength(100).WithMessage("Position must not exceed 100 characters.");

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.");

            RuleFor(x => x.HireDate)
                .NotEmpty().WithMessage("Hire date is required.");

            RuleFor(x => x.EmploymentType)
                .NotEmpty().WithMessage("Employment type is required.")
                .Must(t => AllowedEmploymentTypes.Contains(t))
                .WithMessage($"Employment type must be one of: {string.Join(", ", AllowedEmploymentTypes)}.");
        }
    }
}
