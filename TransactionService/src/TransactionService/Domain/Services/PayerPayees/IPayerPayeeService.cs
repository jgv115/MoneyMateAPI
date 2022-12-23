using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Dtos;
using TransactionService.ViewModels;

namespace TransactionService.Domain.Services.PayerPayees
{
    public interface IPayerPayeeService
    {
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayers(int offset, int limit);
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayees(int offset, int limit);
        public Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId);
        public Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payerName);
        public Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee);
        public Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee);
    }
}