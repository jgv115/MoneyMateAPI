using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.Dtos;
using TransactionService.Models;
using TransactionService.Repositories;

namespace TransactionService.Domain
{
    public class TransactionHelperService: ITransactionHelperService
    {
        private readonly CurrentUserContext _userContext;
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;
        
        public TransactionHelperService(CurrentUserContext userContext, ITransactionRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        public Task<List<Transaction>> GetAllTransactionsAsync(DateTime start, DateTime end)
        {
            return _repository.GetAllTransactionsAsync(_userContext.UserId, start, end);
        }

        public Task StoreTransaction(StoreTransactionDto transactionDto)
        {
            var transaction = _mapper.Map<Transaction>(transactionDto);
            transaction.TransactionId = Guid.NewGuid().ToString();
            transaction.UserId = _userContext.UserId;
            transaction.Date = DateTime.UtcNow.ToString("O");
            return _repository.StoreTransaction(transaction);
        }
    }
}