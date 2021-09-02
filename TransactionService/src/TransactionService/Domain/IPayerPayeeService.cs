using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Dtos;
using TransactionService.Models;

namespace TransactionService.Domain
{
    public interface IPayerPayeeService
    {
        public Task<IEnumerable<PayerPayee>> GetPayers();
        public Task<IEnumerable<PayerPayee>> GetPayees();
        public Task<PayerPayee> GetPayer(Guid payerPayeeId);
        public Task<PayerPayee> GetPayee(Guid payerPayeeId);
        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string payerName);
        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string payerName);
        public Task CreatePayer(CreatePayerPayeeDto newPayerPayee);
        public Task CreatePayee(CreatePayerPayeeDto newPayerPayee);
    }
}