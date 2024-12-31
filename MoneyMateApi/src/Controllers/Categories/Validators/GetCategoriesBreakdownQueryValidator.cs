using FluentValidation;
using MoneyMateApi.Controllers.Categories.Dtos;

namespace MoneyMateApi.Controllers.Categories.Validators
{
    public class GetCategoriesBreakdownQueryValidator : AbstractValidator<GetCategoriesBreakdownQuery>
    {
        public GetCategoriesBreakdownQueryValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(query => query.Type).NotNull();

            When(query => query.End.HasValue && query.Start.HasValue, () =>
            {
                RuleFor(query => query.Frequency).Empty()
                    .WithMessage("Frequency must be empty if start and end is defined");
                RuleFor(query => query.Periods).Empty().WithMessage("Periods must be empty if start and end is defined");
            });

            When(query => !string.IsNullOrWhiteSpace(query.Frequency) || query.Periods.HasValue, () =>
            {
                RuleFor(query => query.Start).Empty()
                    .WithMessage("Start must be empty if frequency and periods is defined");
                RuleFor(query => query.End).Empty().WithMessage("End must be empty if frequency and periods is defined");
            });

            When(query => query.Start.HasValue, () =>
            {
                RuleFor(query => query.End).NotEmpty().NotNull()
                    .WithMessage("End must be defined if start is defined");
            });

            When(query => query.End.HasValue, () =>
            {
                RuleFor(query => query.Start).NotEmpty().NotNull()
                    .WithMessage("Start must be defined if start is defined");
            });

            When(query => !string.IsNullOrWhiteSpace(query.Frequency), () =>
            {
                RuleFor(query => query.Periods).NotEmpty().WithMessage("Periods must be defined if frequency is defined");
            });
            
            When(query => query.Periods.HasValue, () =>
            {
                RuleFor(query => query.Frequency).NotEmpty().WithMessage("Frequency must be defined if periods is defined");
            });
        }
    }
}