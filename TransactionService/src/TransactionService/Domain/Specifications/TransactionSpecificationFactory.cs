using System.Collections.Generic;
using System.Linq;
using TransactionService.Dtos;

namespace TransactionService.Domain.Specifications
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

            if (query.SubcategoriesQuery.Any())
            {
                specifications.Add(new SubcategoriesSpec(query.SubcategoriesQuery));
            }

            return new AndSpec(specifications);
        }
    }
}