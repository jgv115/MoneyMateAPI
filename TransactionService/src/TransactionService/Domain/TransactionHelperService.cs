using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Repositories;

namespace TransactionService.Domain
{
    public class TransactionHelperService: ITransactionHelperService
    {
        private readonly ITransactionRepository _repository;
        
        public TransactionHelperService(ITransactionRepository repository)
        {
            _repository = repository;
        }
        
        public Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return _repository.GetAllTransactionsAsync();
        }
    }
}