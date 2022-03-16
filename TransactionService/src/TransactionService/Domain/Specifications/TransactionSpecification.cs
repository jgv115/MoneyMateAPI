using System;
using System.Collections.Generic;
using System.Linq;
using TransactionService.Constants;
using TransactionService.Domain.Models;

namespace TransactionService.Domain.Specifications
{
    public class AndSpec : ITransactionSpecification
    {
        private readonly IEnumerable<ITransactionSpecification> _specifications;

        public AndSpec(IEnumerable<ITransactionSpecification> specifications)
        {
            _specifications = specifications;
        }

        public bool IsSatisfied(Transaction item)
        {
            return _specifications.All(spec => spec.IsSatisfied(item));
        }
    }

    public class TransactionTypeSpec : ITransactionSpecification
    {
        private readonly TransactionType _transactionType;

        public TransactionTypeSpec(TransactionType transactionType)
        {
            _transactionType = transactionType;
        }

        public bool IsSatisfied(Transaction item)
        {
            return item.TransactionType.Equals(_transactionType.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CategoriesSpec : ITransactionSpecification
    {
        private readonly IEnumerable<string> _categories;

        public CategoriesSpec(IEnumerable<string> categories)
        {
            _categories = categories;
        }

        public bool IsSatisfied(Transaction item)
        {
            return _categories.Contains(item.Category);
        }
    }
    
    public class SubcategoriesSpec : ITransactionSpecification
    {
        private readonly IEnumerable<string> _categories;

        public SubcategoriesSpec(IEnumerable<string> categories)
        {
            _categories = categories;
        }

        public bool IsSatisfied(Transaction item)
        {
            return _categories.Contains(item.Subcategory);
        }
    }
}