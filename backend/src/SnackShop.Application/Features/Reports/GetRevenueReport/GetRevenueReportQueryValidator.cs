using FluentValidation;

namespace SnackShop.Application.Features.Reports.GetRevenueReport;

public sealed class GetRevenueReportQueryValidator : AbstractValidator<GetRevenueReportQuery>
{
    public const int MaxDays = 366;

    public GetRevenueReportQueryValidator()
    {
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From);
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber < MaxDays)
            .WithName("to").WithMessage($"The range can cover at most {MaxDays} days.");
    }
}
