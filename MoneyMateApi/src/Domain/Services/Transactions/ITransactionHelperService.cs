using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Domain.Services.Transactions
{
    public interface ITransactionHelperService
    {
        public Task<Transaction> GetTransactionById(string transactionId);
        public Task<List<Transaction>> GetTransactionsAsync(GetTransactionsQuery queryParams);
        public Task StoreTransaction(StoreTransactionDto transactionDto);
        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto);
        public Task DeleteTransaction(string transactionId);
    }
}