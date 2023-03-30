using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;

namespace TransactionService.Domain.Services.Transactions
{
    public interface ITransactionHelperService
    {
        public Task<List<Transaction>> GetTransactionsAsync(GetTransactionsQuery queryParams);
        public Task StoreTransaction(StoreTransactionDto transactionDto);
        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto);
        public Task DeleteTransaction(string transactionId);
    }
}