using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.ViewModels;

namespace TransactionService.Domain.Services
{
    public interface IPayerPayeeService
    {
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayers();
        public Task<IEnumerable<PayerPayeeViewModel>> GetPayees();
        public Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId);
        public Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName);
        public Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payerName);
        public Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee);
        public Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee);
    }
}