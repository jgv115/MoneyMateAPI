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
                    RuleFor(query => query.SubcategoriesQuery).Empty()
                        .WithMessage("SubcategoriesQuery must be empty when categories are provided");
                });
            When(query => query.SubcategoriesQuery.Any(),
                () =>
                {
                    RuleFor(query => query.Categories).Empty()
                        .WithMessage("Categories must be empty when SubcategoriesQuery is provided");
                });
        }
    }
}