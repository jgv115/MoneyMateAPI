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

        public Task CreatePayer(CreatePayerPayeeDto newPayerPayee)
        {
            return _repository.CreatePayer(new PayerPayee
            {
                Name = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });
        }

        public Task CreatePayee(CreatePayerPayeeDto newPayerPayee)
        {
            return _repository.CreatePayee(new PayerPayee
            {
                Name = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });
        }
    }
}