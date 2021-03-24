using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Dtos;
using TransactionService.Models;

namespace TransactionService.Domain
{
    public interface ITransactionHelperService
    {
        public Task<List<Transaction>> GetAllTransactionsAsync(DateTime start, DateTime end);
        public Task StoreTransaction(StoreTransactionDto transactionDto);
    }
}