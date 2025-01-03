using System;
using System.Collections.Generic;
using System.Linq;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Domain.Services.Transactions.Specifications
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
            return item.TransactionType.Equals(_transactionType.ToProperString(), StringComparison.OrdinalIgnoreCase);
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
        private readonly IEnumerable<string> _subcategories;

        public SubcategoriesSpec(IEnumerable<string> subcategories)
        {
            _subcategories = subcategories;
        }

        public bool IsSatisfied(Transaction item)
        {
            return _subcategories.Contains(item.Subcategory);
        }
    }

    public class PayerPayeeIdsSpec : ITransactionSpecification
    {
        private readonly IEnumerable<string> _payerPayeeIds;
    
        public PayerPayeeIdsSpec(IEnumerable<string> payerPayeeIds)
        {
            _payerPayeeIds = payerPayeeIds;
        }
    
        public bool IsSatisfied(Transaction item)
        {
            return _payerPayeeIds.Contains(item.PayerPayeeId);
        }
    }
}