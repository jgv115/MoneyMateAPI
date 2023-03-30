using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Repositories.DynamoDb.Models;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<List<Transaction>> GetTransactions(DateRange dateRange,
            ITransactionSpecification spec);

        public Task StoreTransaction(Transaction newTransaction);
        public Task PutTransaction(Transaction newTransaction);
        public Task DeleteTransaction(string transactionId);
    }
}