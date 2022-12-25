using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.ViewModels;

namespace TransactionService.Domain.Services.PayerPayees
{
    public class PayerPayeeService : IPayerPayeeService
    {
        private readonly CurrentUserContext _userContext;
        private readonly IPayerPayeeRepository _repository;
        private readonly IPayerPayeeEnricher _payerPayeeEnricher;

        public PayerPayeeService(CurrentUserContext userContext, IPayerPayeeRepository repo,
            IPayerPayeeEnricher payerPayeeEnricher)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _repository = repo ?? throw new ArgumentNullException(nameof(repo));
            _payerPayeeEnricher = payerPayeeEnricher;
        }

        private async Task<IEnumerable<PayerPayeeViewModel>> EnrichAndMapPayerPayeesToViewModels(
            IEnumerable<PayerPayee> payerPayees)
        {
            var enrichTasks = payerPayees.Select(payerPayee => EnrichAndMapPayerPayeeToViewModel(payerPayee));
            var results = await Task.WhenAll(enrichTasks);
            return results.ToList();
        }

        private async Task<PayerPayeeViewModel> EnrichAndMapPayerPayeeToViewModel(PayerPayee payerPayee)
        {
            if (string.IsNullOrEmpty(payerPayee.ExternalId))
                return new PayerPayeeViewModel
                {
                    ExternalId = payerPayee.ExternalId,
                    PayerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                    PayerPayeeName = payerPayee.PayerPayeeName
                };

            var details = await _payerPayeeEnricher.GetExtraPayerPayeeDetails(payerPayee.ExternalId);
            return new PayerPayeeViewModel
            {
                ExternalId = payerPayee.ExternalId,
                PayerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                PayerPayeeName = payerPayee.PayerPayeeName,
                Address = details.Address
            };
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayers(int offset, int limit)
        {
            var payers = await _repository.GetPayers(_userContext.UserId, new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });
            return await EnrichAndMapPayerPayeesToViewModels(payers);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayees(int offset, int limit)
        {
            var payees = await _repository.GetPayees(_userContext.UserId, new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });
            return await EnrichAndMapPayerPayeesToViewModels(payees);
        }

        public async Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId)
        {
            var payer = await _repository.GetPayer(_userContext.UserId, payerPayeeId);
            return await EnrichAndMapPayerPayeeToViewModel(payer);
        }

        public async Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId)
        {
            var payee = await _repository.GetPayee(_userContext.UserId, payerPayeeId);
            return await EnrichAndMapPayerPayeeToViewModel(payee);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName)
        {
            var payers = await _repository.AutocompletePayer(_userContext.UserId, payerName);
            return await EnrichAndMapPayerPayeesToViewModels(payers);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payeeName)
        {
            var payees = await _repository.AutocompletePayee(_userContext.UserId, payeeName);
            return await EnrichAndMapPayerPayeesToViewModels(payees);
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

            if (!string.IsNullOrEmpty(newPayerPayee.ExternalId))
                return await EnrichAndMapPayerPayeeToViewModel(new PayerPayee
                {
                    PayerPayeeId = payerPayeeId.ToString(),
                    PayerPayeeName = newPayerPayee.Name,
                    ExternalId = newPayerPayee.ExternalId,
                    UserId = _userContext.UserId
                });

            return new PayerPayeeViewModel
            {
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