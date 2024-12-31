using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.PayerPayees;

namespace MoneyMateApi.Repositories
{
    public interface IPayerPayeeRepository
    {
        public Task<IEnumerable<PayerPayee>> GetPayers(PaginationSpec paginationSpec);
        public Task<IEnumerable<PayerPayee>> GetPayees(PaginationSpec paginationSpec);
        public Task<PayerPayee> GetPayer(Guid payerPayeeId);
        public Task<PayerPayee> GetPayee(Guid payerPayeeId);
        public Task CreatePayerOrPayee(PayerPayeeType type, PayerPayee newPayerPayee);
        public Task<IEnumerable<PayerPayee>> FindPayer(string searchQuery);
        public Task<IEnumerable<PayerPayee>> FindPayee(string searchQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string autocompleteQuery);
        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string autocompleteQuery);

        public Task<IEnumerable<PayerPayee>> GetSuggestedPayersOrPayees(PayerPayeeType payerpayeeType,
            IPayerPayeeSuggestionParameters suggestionParameters,
            int limit = 20);

        public Task PutPayerOrPayee(PayerPayeeType type, PayerPayee newPayerPayee);
        public Task DeletePayer(string userId);
        public Task DeletePayee(string userId);
    }
}