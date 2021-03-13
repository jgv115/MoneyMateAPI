using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public interface ITransactionRepository
    {
        public Task<List<Transaction>> GetAllTransactionsAsync();
    }
}