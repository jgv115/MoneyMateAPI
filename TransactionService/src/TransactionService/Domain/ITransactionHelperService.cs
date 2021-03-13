using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Domain
{
    public interface ITransactionHelperService
    {
        public Task<List<Transaction>> GetAllTransactionsAsync();
    }
}