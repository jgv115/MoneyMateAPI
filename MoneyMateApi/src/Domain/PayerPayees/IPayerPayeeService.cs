using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.Dtos;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;

namespace MoneyMateApi.Domain.PayerPayees
{
    public interface IPayerPayeeService
    {
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayers(int offset, int limit, bool includeEnrichedData);
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayees(int offset, int limit, bool includeEnrichedData);
        public Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId);
        public Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payerName);

        public Task<IEnumerable<PayerPayeeViewModel>> GetSuggestedPayersOrPayees(PayerPayeeType payerPayeeType, SuggestionPromptDto suggestionPromptDto, bool includeEnrichedData);
        
        public Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee);
        public Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee);
    }
}