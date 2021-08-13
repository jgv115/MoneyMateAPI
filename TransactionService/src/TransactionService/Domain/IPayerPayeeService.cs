using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Domain
{
    public interface IPayerPayeeService
    {
        public Task<IEnumerable<PayerPayee>> GetPayers();
        public Task<IEnumerable<PayerPayee>> GetPayees();
    }
}