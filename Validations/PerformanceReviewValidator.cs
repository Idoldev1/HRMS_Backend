using FluentValidation;
using HRMS.API.Models;

namespace HRMS.API.Validations
{
    public class PerformanceReviewValidator : AbstractValidator<PerformanceReview>
    {
        private static readonly string[] AllowedStatuses =
            { "Draft", "Submitted", "Acknowledged" };

        public PerformanceReviewValidator()
        {
            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("A valid employee is required.");

            RuleFor(x => x.ReviewedById)
                .GreaterThan(0).WithMessage("Reviewer is required.");

            RuleFor(x => x.ReviewPeriodStartDate)
                .NotEmpty().WithMessage("Review period start date is required.");

            RuleFor(x => x.ReviewPeriodEndDate)
                .NotEmpty().WithMessage("Review period end date is required.")
                .GreaterThanOrEqualTo(x => x.ReviewPeriodStartDate)
                .WithMessage("Review period end date must be on or after the start date.");

            RuleFor(x => x.OverallRating)
                .InclusiveBetween(1, 5).WithMessage("Overall rating must be between 1 and 5.");

            RuleFor(x => x.QualityRating)
                .InclusiveBetween(1, 5).WithMessage("Quality rating must be between 1 and 5.")
                .When(x => x.QualityRating.HasValue);

            RuleFor(x => x.ProductivityRating)
                .InclusiveBetween(1, 5).WithMessage("Productivity rating must be between 1 and 5.")
                .When(x => x.ProductivityRating.HasValue);

            RuleFor(x => x.CommunicationRating)
                .InclusiveBetween(1, 5).WithMessage("Communication rating must be between 1 and 5.")
                .When(x => x.CommunicationRating.HasValue);

            RuleFor(x => x.TeamworkRating)
                .InclusiveBetween(1, 5).WithMessage("Teamwork rating must be between 1 and 5.")
                .When(x => x.TeamworkRating.HasValue);

            RuleFor(x => x.LeadershipRating)
                .InclusiveBetween(1, 5).WithMessage("Leadership rating must be between 1 and 5.")
                .When(x => x.LeadershipRating.HasValue);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(s => AllowedStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }
    }
}
