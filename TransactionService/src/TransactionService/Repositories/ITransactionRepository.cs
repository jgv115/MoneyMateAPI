using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<List<Transaction>> GetAllTransactionsAsync(string userId,  DateTime start, DateTime end);
        public Task StoreTransaction(Transaction newTransaction);
        public Task PutTransaction(Transaction newTransaction);
        public Task DeleteTransaction(string userId, string transactionId);
    }
}