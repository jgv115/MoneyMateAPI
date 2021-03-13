using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<List<Transaction>> GetAllTransactionsAsync();
    }
}