using FluentValidation;
using HRMS.API.Contracts.Attendance;

namespace HRMS.API.Validations
{
    public class GetAttendancesRequestValidator : AbstractValidator<GetAttendancesRequest>
    {
        public GetAttendancesRequestValidator()
        {
            RuleFor(x => x.EndDate)
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after the start date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
        }
    }
}
