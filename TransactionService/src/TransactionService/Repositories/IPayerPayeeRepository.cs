using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface IPayerPayeeRepository
    {
        public Task<IEnumerable<PayerPayee>> GetPayers(string userId);
        public Task<IEnumerable<PayerPayee>> GetPayees(string userId);
        public Task<PayerPayee> GetPayer(string userId, Guid payerPayeeId);
        public Task<PayerPayee> GetPayee(string userId, Guid payerPayeeId);
        public Task CreatePayer(PayerPayee newPayerPayee);
        public Task CreatePayee(PayerPayee newPayerPayee);
        public Task<IEnumerable<PayerPayee>> FindPayer(string userId, string searchQuery);
        public Task<IEnumerable<PayerPayee>> FindPayee(string userId, string searchQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string userId, string autocompleteQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string userId, string autocompleteQuery);
        public Task PutPayer(string userId);
        public Task PutPayee(string userId);
        public Task DeletePayer(string userId);
        public Task DeletePayee(string userId);
    }
}