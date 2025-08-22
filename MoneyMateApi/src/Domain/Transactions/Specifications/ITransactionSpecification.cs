using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Domain.Services.Transactions.Specifications
{
    public interface ITransactionSpecification
    {
        public bool IsSatisfied(Transaction item);
    }
}