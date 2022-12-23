using System.Collections.Generic;
using System.Linq;
using TransactionService.Dtos;

namespace TransactionService.Domain.Services.Transactions.Specifications
{
    public class TransactionSpecificationFactory
    {
        public ITransactionSpecification Create(GetTransactionsQuery query)
        {
            var specifications = new List<ITransactionSpecification>();
            if (query.Type.HasValue)
            {
                specifications.Add(new TransactionTypeSpec(query.Type.Value));
            }

            if (query.Categories.Any())
            {
                specifications.Add(new CategoriesSpec(query.Categories));
            }

            if (query.Subcategories.Any())
            {
                specifications.Add(new SubcategoriesSpec(query.Subcategories));
            }

            if (query.PayerPayeeIds.Any())
            {
                specifications.Add(new PayerPayeeIdsSpec(query.PayerPayeeIds));
            }

            return new AndSpec(specifications);
        }
    }
}