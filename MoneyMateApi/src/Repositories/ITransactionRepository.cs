using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;

namespace MoneyMateApi.Repositories
{
    public interface ITransactionRepository
    {
        public Task<Transaction> GetTransactionById(string transactionId);
        public Task<IEnumerable<Transaction>> GetTransactions(DateRange dateRange,
            ITransactionSpecification spec);

        public Task StoreTransaction(Transaction newTransaction);
        public Task PutTransaction(Transaction newTransaction);
        public Task DeleteTransaction(string transactionId);
    }
}