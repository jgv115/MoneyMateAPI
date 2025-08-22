using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Controllers.Transactions.Dtos;

namespace MoneyMateApi.Domain.Services.Transactions
{
    public interface ITransactionHelperService
    {
        public Task<TransactionOutputDto> GetTransactionById(string transactionId);
        public Task<IEnumerable<TransactionOutputDto>> GetTransactionsAsync(GetTransactionsQuery queryParams);
        public Task StoreTransaction(StoreTransactionDto transactionDto);
        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto);
        public Task DeleteTransaction(string transactionId);
    }
}