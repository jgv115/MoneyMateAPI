using FluentValidation;
using MoneyMateApi.Controllers.Categories.Dtos;

namespace MoneyMateApi.Controllers.Categories.Validators
{
    public class GetSubcategoriesBreakdownQueryValidator : AbstractValidator<GetSubcategoriesBreakdownQuery>
    {
        public GetSubcategoriesBreakdownQueryValidator()
        {
            CascadeMode = CascadeMode.Stop;
            
            RuleFor(query => query.Category).NotEmpty().NotNull();

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
        }
    }
}