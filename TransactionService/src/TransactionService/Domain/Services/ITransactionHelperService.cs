using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;

namespace TransactionService.Domain.Services
{
    public interface ITransactionHelperService
    {
        public Task<List<Transaction>> GetAllTransactionsAsync(DateTime start, DateTime end, string type = null);

        public Task<List<Transaction>> GetAllTransactionsByCategoryAsync(string categoryName, DateTime start,
            DateTime end);

        public Task StoreTransaction(StoreTransactionDto transactionDto);
        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto);
        public Task DeleteTransaction(string transactionId);
    }
}