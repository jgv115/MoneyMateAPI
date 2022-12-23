using TransactionService.Domain.Models;

namespace TransactionService.Domain.Services.Transactions.Specifications
{
    public interface ITransactionSpecification
    {
        public bool IsSatisfied(Transaction item);
    }
}