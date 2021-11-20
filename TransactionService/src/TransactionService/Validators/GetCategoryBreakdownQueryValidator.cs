using System.Data;
using FluentValidation;
using TransactionService.Dtos;

namespace TransactionService.Validators
{
    public class GetCategoryBreakdownQueryValidator: AbstractValidator<GetCategoryBreakdownQuery>
    {
        public GetCategoryBreakdownQueryValidator()
        {

            RuleFor(query => query.Type).NotEmpty().NotNull();
            // RuleFor(query =>)

            When(query => query.Start.HasValue, () =>
            {
                RuleFor(query => query.End).NotEmpty().NotNull();
            });
        }
    }
}