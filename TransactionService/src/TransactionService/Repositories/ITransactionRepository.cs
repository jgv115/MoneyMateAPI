using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<List<Transaction>> GetAllTransactionsAsync(string userId, DateTime start, DateTime end);

        public Task<List<Transaction>> GetAllTransactionsAsync(string userId, DateTime start, DateTime end,
            string transactionType);

        public Task<List<Transaction>> GetAllTransactionsByCategoryAsync(string userId, string categoryName,
            DateTime start, DateTime end);

        public Task StoreTransaction(Transaction newTransaction);
        public Task PutTransaction(Transaction newTransaction);
        public Task DeleteTransaction(string userId, string transactionId);
    }
}