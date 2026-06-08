using FluentValidation;
using HRMS.API.Contracts.Timesheet;

namespace HRMS.API.Validations
{
    public class GenerateTimesheetRequestValidator : AbstractValidator<GenerateTimesheetRequest>
    {
        public GenerateTimesheetRequestValidator()
        {
            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");

            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.")
                .Must((req, year) =>
                {
                    var requestedDate = new DateTime(year, req.Month, 1);
                    return requestedDate <= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                })
                .WithMessage("Cannot generate a timesheet for a future month.");
        }
    }

    public class ApproveTimesheetRequestValidator : AbstractValidator<ApproveTimesheetRequest>
    {
        public ApproveTimesheetRequestValidator()
        {
            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.")
                .When(x => x.Comment != null);
        }
    }

    public class RejectTimesheetRequestValidator : AbstractValidator<RejectTimesheetRequest>
    {
        public RejectTimesheetRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("A rejection reason is required.")
                .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters.");
        }
    }

    public class UpdateTimesheetEntriesRequestValidator : AbstractValidator<UpdateTimesheetEntriesRequest>
    {
        public UpdateTimesheetEntriesRequestValidator()
        {
            RuleFor(x => x.Entries)
                .NotNull().WithMessage("Entries list is required.")
                .NotEmpty().WithMessage("At least one entry must be provided.");

            RuleForEach(x => x.Entries).SetValidator(new UpdateTimesheetEntryRequestValidator());
        }
    }

    public class UpdateTimesheetEntryRequestValidator : AbstractValidator<UpdateTimesheetEntryRequest>
    {
        public UpdateTimesheetEntryRequestValidator()
        {
            RuleFor(x => x.EntryId)
                .GreaterThan(0).WithMessage("A valid entry ID is required.");

            RuleFor(x => x.TasksCompleted)
                .MaximumLength(2000).WithMessage("Tasks completed must not exceed 2000 characters.")
                .When(x => x.TasksCompleted != null);

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters.")
                .When(x => x.Notes != null);
        }
    }
}
