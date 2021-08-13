using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    }
}