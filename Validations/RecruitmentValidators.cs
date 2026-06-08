using FluentValidation;
using HRMS.API.Contracts.Recruitment;

namespace HRMS.API.Validations
{
    public class CreateJobPostingRequestValidator : AbstractValidator<CreateJobPostingRequest>
    {
        private static readonly string[] AllowedStatuses = { "Draft", "Open", "Closed" };
        private static readonly string[] AllowedEmploymentTypes = { "Full-Time", "Part-Time", "Contract", "Internship" };
        private static readonly string[] AllowedWorkModes = { "On-Site", "Hybrid", "Remote" };

        public CreateJobPostingRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Job title is required.")
                .MaximumLength(200);
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Job description is required.");
            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required.")
                .MaximumLength(100);
            RuleFor(x => x.EmploymentType)
                .NotEmpty()
                .Must(t => AllowedEmploymentTypes.Contains(t))
                .WithMessage($"Employment type must be one of: {string.Join(", ", AllowedEmploymentTypes)}.");
            RuleFor(x => x.WorkMode)
                .NotEmpty()
                .Must(w => AllowedWorkModes.Contains(w))
                .WithMessage($"Work mode must be one of: {string.Join(", ", AllowedWorkModes)}.");
            RuleFor(x => x.Status)
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
            RuleFor(x => x.SalaryMin)
                .GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue);
            RuleFor(x => x.SalaryMax)
                .GreaterThanOrEqualTo(x => x.SalaryMin ?? 0).When(x => x.SalaryMax.HasValue)
                .WithMessage("Maximum salary must be greater than or equal to minimum salary.");
        }
    }

    public class UpdateJobPostingRequestValidator : AbstractValidator<UpdateJobPostingRequest>
    {
        private static readonly string[] AllowedStatuses = { "Draft", "Open", "Closed" };
        private static readonly string[] AllowedEmploymentTypes = { "Full-Time", "Part-Time", "Contract", "Internship" };
        private static readonly string[] AllowedWorkModes = { "On-Site", "Hybrid", "Remote" };

        public UpdateJobPostingRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Job title is required.")
                .MaximumLength(200);
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Job description is required.");
            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required.")
                .MaximumLength(100);
            RuleFor(x => x.EmploymentType)
                .NotEmpty()
                .Must(t => AllowedEmploymentTypes.Contains(t))
                .WithMessage($"Employment type must be one of: {string.Join(", ", AllowedEmploymentTypes)}.");
            RuleFor(x => x.WorkMode)
                .NotEmpty()
                .Must(w => AllowedWorkModes.Contains(w))
                .WithMessage($"Work mode must be one of: {string.Join(", ", AllowedWorkModes)}.");
            RuleFor(x => x.Status)
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
            RuleFor(x => x.SalaryMin)
                .GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue);
            RuleFor(x => x.SalaryMax)
                .GreaterThanOrEqualTo(x => x.SalaryMin ?? 0).When(x => x.SalaryMax.HasValue)
                .WithMessage("Maximum salary must be greater than or equal to minimum salary.");
        }
    }

    public class UpdateJobStatusRequestValidator : AbstractValidator<UpdateJobStatusRequest>
    {
        private static readonly string[] AllowedStatuses = { "Draft", "Open", "Closed" };

        public UpdateJobStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }
    }

    public class UpdateApplicationStatusRequestValidator : AbstractValidator<UpdateApplicationStatusRequest>
    {
        private static readonly string[] AllowedStatuses = { "Applied", "Shortlisted", "Interview", "Rejected", "Hired" };

        public UpdateApplicationStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }
    }

    public class ApplyForJobRequestValidator : AbstractValidator<ApplyForJobRequest>
    {
        public ApplyForJobRequestValidator()
        {
            RuleFor(x => x.CandidateName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Candidate name is required.")
                .Must(name => name == null || name.Trim().Length <= 100)
                .WithMessage("The length of 'Candidate Name' must be 100 characters or fewer.");

            RuleFor(x => x.CandidateEmail)
                .Must(email => !string.IsNullOrWhiteSpace(email))
                .WithMessage("Candidate email is required.")
                .Must(email => email == null || email.Trim().Length <= 200)
                .WithMessage("The length of 'Candidate Email' must be 200 characters or fewer.")
                .Must(email => string.IsNullOrWhiteSpace(email) || new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email.Trim()))
                .WithMessage("Candidate email is not valid.");

            RuleFor(x => x.CandidatePhone)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.CandidatePhone));
        }
    }
}
