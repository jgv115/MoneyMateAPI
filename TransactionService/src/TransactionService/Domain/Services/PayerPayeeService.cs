using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.ViewModels;

namespace TransactionService.Domain.Services
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

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayers()
        {
            var payers = await _repository.GetPayers(_userContext.UserId);
            return payers.Select(payer => new PayerPayeeViewModel
            {
                ExternalId = payer.ExternalId,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                PayerPayeeName = payer.PayerPayeeName
            });
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayees()
        {
            var payees = await _repository.GetPayees(_userContext.UserId);
            return payees.Select(payer => new PayerPayeeViewModel
            {
                ExternalId = payer.ExternalId,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                PayerPayeeName = payer.PayerPayeeName
            });
        }

        public async Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId)
        {
            var payer = await _repository.GetPayer(_userContext.UserId, payerPayeeId);
            return new PayerPayeeViewModel
            {
                ExternalId = payer.ExternalId,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                PayerPayeeName = payer.PayerPayeeName
            };
        }

        public async Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId)
        {
            var payee = await _repository.GetPayee(_userContext.UserId, payerPayeeId);
            return new PayerPayeeViewModel
            {
                ExternalId = payee.ExternalId,
                PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                PayerPayeeName = payee.PayerPayeeName
            };
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string payerName)
        {
            return _repository.AutocompletePayer(_userContext.UserId, payerName);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string payeeName)
        {
            return _repository.AutocompletePayee(_userContext.UserId, payeeName);
        }

        public async Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid();
            await _repository.CreatePayer(new PayerPayee
            {
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });

            return new PayerPayeeViewModel
            {
                ExternalId = newPayerPayee.ExternalId,
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name
            };
        }

        public async Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid();
            await _repository.CreatePayee(new PayerPayee
            {
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
                UserId = _userContext.UserId
            });

            return new PayerPayeeViewModel
            {
                ExternalId = newPayerPayee.ExternalId,
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name
            };
        }
    }
}