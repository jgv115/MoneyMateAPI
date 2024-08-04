using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher;

namespace TransactionService.Domain.Services.PayerPayees
{
    public class PayerPayeeService : IPayerPayeeService
    {
        private readonly IPayerPayeeRepository _repository;
        private readonly IPayerPayeeEnricher _payerPayeeEnricher;

        public PayerPayeeService(IPayerPayeeRepository repo,
            IPayerPayeeEnricher payerPayeeEnricher)
        {
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

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayers(int offset, int limit,
            bool includeEnrichedData = true)
        {
            var payers = await _repository.GetPayers(new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });
            if (includeEnrichedData)
                return await EnrichAndMapPayerPayeesToViewModels(payers);
            else
                return payers.Select(payer => new PayerPayeeViewModel
                {
                    PayerPayeeName = payer.PayerPayeeName,
                    PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                    ExternalId = payer.ExternalId
                });
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetPayees(int offset, int limit,
            bool includeEnrichedData = true)
        {
            var payees = await _repository.GetPayees(new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });

            if (includeEnrichedData)
                return await EnrichAndMapPayerPayeesToViewModels(payees);
            else
                return payees.Select(payee => new PayerPayeeViewModel
                {
                    PayerPayeeName = payee.PayerPayeeName,
                    PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                    ExternalId = payee.ExternalId
                });
        }

        public async Task<PayerPayeeViewModel> GetPayer(Guid payerPayeeId)
        {
            var payer = await _repository.GetPayer(payerPayeeId);
            return await EnrichAndMapPayerPayeeToViewModel(payer);
        }

        public async Task<PayerPayeeViewModel> GetPayee(Guid payerPayeeId)
        {
            var payee = await _repository.GetPayee(payerPayeeId);
            return await EnrichAndMapPayerPayeeToViewModel(payee);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayer(string payerName)
        {
            var payers = await _repository.AutocompletePayer(payerName);
            return await EnrichAndMapPayerPayeesToViewModels(payers);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> AutocompletePayee(string payeeName)
        {
            var payees = await _repository.AutocompletePayee(payeeName);
            return await EnrichAndMapPayerPayeesToViewModels(payees);
        }

        public async Task<IEnumerable<PayerPayeeViewModel>> GetSuggestedPayersOrPayees(PayerPayeeType payerPayeeType,
            SuggestionPromptDto suggestionPromptDto, bool includeEnrichedData = false)
        {
            var suggestionFactory = new PayerPayeeSuggestionParameterFactory();
            var suggestionParameters = suggestionFactory.Generate(suggestionPromptDto);

            var suggestedPayersOrPayees =
                await _repository.GetSuggestedPayersOrPayees(payerPayeeType, suggestionParameters);

            if (includeEnrichedData)
            {
                var enrichTasks = suggestedPayersOrPayees.Select(payerPayee =>
                    _payerPayeeEnricher.EnrichPayerPayeeToViewModel(payerPayeeType, payerPayee));
                var results = await Task.WhenAll(enrichTasks);
                return results.ToList();
            }

            return suggestedPayersOrPayees.Select(payerPayee => new PayerPayeeViewModel
            {
                PayerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                PayerPayeeName = payerPayee.PayerPayeeName
            }).ToList();
        }

        // TODO: might be some coupling issues here - we are assuming repository will store exactly as we are inputting
        public async Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid().ToString();
            await _repository.CreatePayerOrPayee(PayerPayeeType.Payer, new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId ?? "",
            });

            return await EnrichAndMapPayerPayeeToViewModel(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId ?? "",
            });
        }

        public async Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid().ToString();

            await _repository.CreatePayerOrPayee(PayerPayeeType.Payee, new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId ?? "",
            });

            return await EnrichAndMapPayerPayeeToViewModel(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId ?? "",
            });
        }
    }
}