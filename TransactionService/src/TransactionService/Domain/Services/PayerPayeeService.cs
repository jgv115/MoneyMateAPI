using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Models;
using TransactionService.ViewModels;

namespace TransactionService.Domain.Services
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

        private PayerPayeeViewModel MapPayerPayeeToViewModel(PayerPayee payerPayee)
            => new PayerPayeeViewModel
            {
                ExternalId = payerPayee.ExternalId,
                PayerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                PayerPayeeName = payerPayee.PayerPayeeName
            };

        private PayerPayeeViewModel MapPayerPayeeAndDetailsToViewModel(PayerPayee payerPayee,
            ExtraPayerPayeeDetails details)
            => new PayerPayeeViewModel
            {
                ExternalId = payerPayee.ExternalId,
                PayerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                PayerPayeeName = payerPayee.PayerPayeeName,
                Address = details.Address
            };

        private IEnumerable<PayerPayeeViewModel> MapPayerPayeesToViewModels(IEnumerable<PayerPayee> payerPayees)
            => payerPayees.Select(payer => MapPayerPayeeToViewModel(payer));


        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayers()
        {
            var payers = await _repository.GetPayers(_userContext.UserId);
            return MapPayerPayeesToViewModels(payers);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayees()
        {
            var payees = await _repository.GetPayees(_userContext.UserId);
            return MapPayerPayeesToViewModels(payees);
        }

        public async Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId)
        {
            var payer = await _repository.GetPayer(_userContext.UserId, payerPayeeId);
            var details = await _payerPayeeEnricher.GetExtraPayerPayeeDetails(payer.ExternalId);
            return MapPayerPayeeAndDetailsToViewModel(payer, details);
        }

        public async Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId)
        {
            var payee = await _repository.GetPayee(_userContext.UserId, payerPayeeId);
            return MapPayerPayeeToViewModel(payee);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName)
        {
            var payers = await _repository.AutocompletePayer(_userContext.UserId, payerName);
            return MapPayerPayeesToViewModels(payers);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payeeName)
        {
            var payees = await _repository.AutocompletePayee(_userContext.UserId, payeeName);
            return MapPayerPayeesToViewModels(payees);
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