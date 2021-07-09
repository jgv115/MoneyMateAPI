using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public interface IPayerPayeeRepository
    {
        public Task<IEnumerable<PayerPayee>> GetPayers(string userId);
        public Task<IEnumerable<PayerPayee>> GetPayees(string userId);
        public Task StorePayer(PayerPayee newPayerPayee);
        public Task StorePayee(PayerPayee newPayerPayee);
        public Task PutPayer(string userId);
        public Task PutPayee(string userId);
        public Task DeletePayer(string userId);
        public Task DeletePayee(string userId);
    }
}