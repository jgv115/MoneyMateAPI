using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface IPayerPayeeRepository
    {
        public Task<IEnumerable<PayerPayee>> GetPayers(PaginationSpec paginationSpec);
        public Task<IEnumerable<PayerPayee>> GetPayees(PaginationSpec paginationSpec);
        public Task<PayerPayee> GetPayer(Guid payerPayeeId);
        public Task<PayerPayee> GetPayee(Guid payerPayeeId);
        public Task CreatePayer(PayerPayee newPayerPayee);
        public Task CreatePayee(PayerPayee newPayerPayee);
        public Task<IEnumerable<PayerPayee>> FindPayer(string searchQuery);
        public Task<IEnumerable<PayerPayee>> FindPayee(string searchQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string autocompleteQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string autocompleteQuery);
        public Task<IEnumerable<PayerPayee>> GetSuggestedPayers(int limit);
        public Task<IEnumerable<PayerPayee>> GetSuggestedPayees(int limit);
        public Task PutPayer(string userId);
        public Task PutPayee(string userId);
        public Task DeletePayer(string userId);
        public Task DeletePayee(string userId);
    }
}