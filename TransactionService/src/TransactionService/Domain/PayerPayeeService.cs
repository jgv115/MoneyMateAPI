using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Dtos;
using TransactionService.Models;
using TransactionService.Repositories;

namespace TransactionService.Domain
{
    public class PayerPayeeService : IPayerPayeeService
    {
        private readonly CurrentUserContext _userContext;
        private readonly IPayerPayeeRepository _repository;

        public PayerPayeeService(CurrentUserContext userContext, IPayerPayeeRepository repo)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _repository = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public Task<IEnumerable<PayerPayee>> GetPayers()
        {
            return _repository.GetPayers(_userContext.UserId);
        }

        public Task<IEnumerable<PayerPayee>> GetPayees()
        {
            return _repository.GetPayees(_userContext.UserId);
        }

        public Task<PayerPayee> GetPayer(Guid payerPayeeId)
        {
            return _repository.GetPayer(_userContext.UserId, payerPayeeId);
        }

        public Task<PayerPayee> GetPayee(Guid payerPayeeId)
        {
            return _repository.GetPayee(_userContext.UserId, payerPayeeId);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string payerName)
        {
            return _repository.AutocompletePayer(_userContext.UserId, payerName);
        }
        
        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string payeeName)
        {
            return _repository.AutocompletePayee(_userContext.UserId, payeeName);
        }

        public Task CreatePayer(CreatePayerPayeeDto newPayerPayee)
        {
            return _repository.CreatePayer(new PayerPayee
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });
        }

        public Task CreatePayee(CreatePayerPayeeDto newPayerPayee)
        {
            return _repository.CreatePayee(new PayerPayee
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });
        }
    }
}