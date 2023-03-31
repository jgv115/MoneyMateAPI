using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;
using TransactionService.Repositories.DynamoDb.Models;
using TransactionService.Services.PayerPayeeEnricher;

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

        private async Task<PayerPayeeViewModel> EnrichAndMapPayerPayeeToViewModel(PayerPayee dynamoDbPayerPayee)
        {
            if (string.IsNullOrEmpty(dynamoDbPayerPayee.ExternalId))
                return new PayerPayeeViewModel
                {
                    ExternalId = dynamoDbPayerPayee.ExternalId,
                    PayerPayeeId = Guid.Parse(dynamoDbPayerPayee.PayerPayeeId),
                    PayerPayeeName = dynamoDbPayerPayee.PayerPayeeName
                };

            var details = await _payerPayeeEnricher.GetExtraPayerPayeeDetails(dynamoDbPayerPayee.ExternalId);
            return new PayerPayeeViewModel
            {
                ExternalId = dynamoDbPayerPayee.ExternalId,
                PayerPayeeId = Guid.Parse(dynamoDbPayerPayee.PayerPayeeId),
                PayerPayeeName = dynamoDbPayerPayee.PayerPayeeName,
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

        // TODO: might be some coupling issues here - we are assuming repository will store exactly as we are inputting
        public async Task<PayerPayeeViewModel> CreatePayer(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid().ToString();
            await _repository.CreatePayer(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
            });

            return await EnrichAndMapPayerPayeeToViewModel(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
            });
        }

        public async Task<PayerPayeeViewModel> CreatePayee(CreatePayerPayeeDto newPayerPayee)
        {
            var payerPayeeId = Guid.NewGuid().ToString();
            
            await _repository.CreatePayee(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
            });

            return await EnrichAndMapPayerPayeeToViewModel(new PayerPayee
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = newPayerPayee.Name,
                ExternalId = newPayerPayee.ExternalId,
            });
        }
    }
}