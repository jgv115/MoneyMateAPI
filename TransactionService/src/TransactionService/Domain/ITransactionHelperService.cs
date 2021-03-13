using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Repositories;

namespace TransactionService.Domain
{
    public interface ITransactionHelperService
    {
        public Task<List<Transaction>> GetAllTransactionsAsync();
    }
}