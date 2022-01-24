using TransactionService.Domain.Models;

namespace TransactionService.Domain.Specifications
{
    public interface ITransactionSpecification
    {
        public bool IsSatisfied(Transaction item);
    }
}