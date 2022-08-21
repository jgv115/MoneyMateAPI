using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Domain.Specifications;
using TransactionService.Dtos;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services
{
    public class TransactionHelperService : ITransactionHelperService
    {
        private readonly CurrentUserContext _userContext;
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;
        private readonly TransactionSpecificationFactory _specificationFactory;

        public TransactionHelperService(CurrentUserContext userContext, ITransactionRepository repository,
            IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _specificationFactory = new TransactionSpecificationFactory();
        }

        public async Task<List<Transaction>> GetTransactionsAsync(GetTransactionsQuery queryParams)
        {
            var dateRange = new DateRange(queryParams.Start.GetValueOrDefault(DateTime.MinValue),
                queryParams.End.GetValueOrDefault(DateTime.MaxValue));
            var spec = _specificationFactory.Create(queryParams);

            var filteredTransactions = await _repository.GetTransactions(dateRange, spec);
            return filteredTransactions;
        }

        public Task StoreTransaction(StoreTransactionDto transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.TransactionId = Guid.NewGuid().ToString();
            transaction.UserId = _userContext.UserId;
            return _repository.StoreTransaction(transaction);
        }

        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto)
        {
            var transaction = _mapper.Map<Transaction>(putTransactionDto);
            transaction.TransactionId = transactionId;
            transaction.UserId = _userContext.UserId;
            return _repository.PutTransaction(transaction);
        }

        public Task DeleteTransaction(string transactionId)
        {
            return _repository.DeleteTransaction(transactionId);
        }
    }
}