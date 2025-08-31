using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Tags;
using MoneyMateApi.Domain.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Repositories;

namespace MoneyMateApi.Domain.Transactions
{
    public static class TransactionMappers
    {
        public static TransactionOutputDto ToTransactionOutputDto(this Transaction transaction,
            IDictionary<Guid, Tag> tagLookup)
        {
            return new TransactionOutputDto
            {
                TransactionId = transaction.TransactionId,
                TransactionTimestamp = transaction.TransactionTimestamp,
                TransactionType = transaction.TransactionType,
                Amount = transaction.Amount,
                Category = transaction.Category,
                Subcategory = transaction.Subcategory,
                PayerPayeeId = transaction.PayerPayeeId,
                PayerPayeeName = transaction.PayerPayeeName,
                Note = transaction.Note,
                Tags = transaction.TagIds.Select(tagId => tagLookup[tagId])
            };
        }
    }

    /// <summary>
    ///  This service acts as a pseudo domain service. It is quite tightly coupled to the application layer due to the use
    ///  of application layer DTOs. As a result, this service is an amalgamation of domain and application logic.
    ///  This is an accepted trade-off to reduce complexity as it is unlikely that the domain layer will be
    ///  reused in another application.
    /// </summary>
    public class TransactionHelperService : ITransactionHelperService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IMapper _mapper;
        private readonly TransactionSpecificationFactory _specificationFactory;

        public TransactionHelperService(ITransactionRepository transactionRepository, ITagRepository tagRepository,
            IMapper mapper)
        {
            _transactionRepository =
                transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _tagRepository = tagRepository;
            _specificationFactory = new TransactionSpecificationFactory();
        }


        public async Task<TransactionOutputDto> GetTransactionById(string transactionId)
        {
            var transaction = await _transactionRepository.GetTransactionById(transactionId);
            var tagLookup = await _tagRepository.GetTags(transaction.TagIds);
            return transaction.ToTransactionOutputDto(tagLookup);
        }

        public async Task<IEnumerable<TransactionOutputDto>> GetTransactionsAsync(GetTransactionsQuery queryParams)
        {
            var dateRange = new DateRange(queryParams.Start.GetValueOrDefault(DateTime.MinValue),
                queryParams.End.GetValueOrDefault(DateTime.MaxValue));
            var spec = _specificationFactory.Create(queryParams);

            var filteredTransactions = await _transactionRepository.GetTransactions(dateRange, spec);

            var tagIds = filteredTransactions.SelectMany(transaction => transaction.TagIds).Distinct();
            var tagLookup = await _tagRepository.GetTags(tagIds);

            return filteredTransactions.Select(transaction => transaction.ToTransactionOutputDto(tagLookup));
        }

        public Task StoreTransaction(StoreTransactionDto transactionDto)
        {
            // TODO: do we need to convert timestamp to UTC?
            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.TransactionId = Guid.NewGuid().ToString();
            return _transactionRepository.StoreTransaction(transaction);
        }

        public Task PutTransaction(string transactionId, PutTransactionDto putTransactionDto)
        {
            var transaction = _mapper.Map<Transaction>(putTransactionDto);
            transaction.TransactionId = transactionId;
            return _transactionRepository.PutTransaction(transaction);
        }

        public Task DeleteTransaction(string transactionId)
        {
            return _transactionRepository.DeleteTransaction(transactionId);
        }
    }
}