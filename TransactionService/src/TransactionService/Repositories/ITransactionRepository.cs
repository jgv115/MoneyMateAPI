using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<Transaction> GetTransactionById(string transactionId);
        public Task<List<Transaction>> GetTransactions(DateRange dateRange,
            ITransactionSpecification spec);

        public Task StoreTransaction(Transaction newDynamoDbTransaction);
        public Task PutTransaction(Transaction newDynamoDbTransaction);
        public Task DeleteTransaction(string transactionId);
    }
}