using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Repositories;

namespace MoneyMateApi.Domain.Services.Transactions
{
    public class TransactionHelperService : ITransactionHelperService
    {
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;
        private readonly TransactionSpecificationFactory _specificationFactory;

        public TransactionHelperService(ITransactionRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _specificationFactory = new TransactionSpecificationFactory();
        }

        public Task<Transaction> GetTransactionById(string transactionId)
        {
            return _repository.GetTransactionById(transactionId);
        }

        public async Task<List<Transaction>> GetTransactionsAsync(GetTransactionsQuery queryParams)
        {
            var dateRange = new DateRange(queryParams.Start.GetValueOrDefault(DateTime.MinValue),
                queryParams.End.GetValueOrDefault(DateTime.MaxValue));
            var spec = _specificationFactory.Create(queryParams);

            var filteredTransactions = await _repository.GetTransactions(dateRange, spec);
            
            // TODO: need to build a way to populate a TransactionViewModel with tag name
            return filteredTransactions;
        }

        public Task StoreTransaction(StoreTransactionDto transactionDto)
        {
            // TODO: do we need to convert timestamp to UTC?
            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.TransactionId = Guid.NewGuid().ToString();
            return _repository.StoreTransaction(transaction);
        }

        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto)
        {
            var transaction = _mapper.Map<Transaction>(putTransactionDto);
            transaction.TransactionId = transactionId;
            return _repository.PutTransaction(transaction);
        }

        public Task DeleteTransaction(string transactionId)
        {
            return _repository.DeleteTransaction(transactionId);
        }
    }
}