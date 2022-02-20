using System.Linq;
using FluentValidation;
using TransactionService.Dtos;

namespace TransactionService.Validators
{
    public class GetTransactionQueryValidator : AbstractValidator<GetTransactionsQuery>
    {
        public GetTransactionQueryValidator()
        {
            CascadeMode = CascadeMode.Stop;

            When(query => query.Categories.Any(),
                () =>
                {
                    RuleFor(query => query.Subcategories).Empty()
                        .WithMessage("Subcategories must be empty when categories are provided");
                });
            When(query => query.Subcategories.Any(),
                () =>
                {
                    RuleFor(query => query.Categories).Empty()
                        .WithMessage("Categories must be empty when Subcategories is provided");
                });
        }
    }
}