using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories.DynamoDb.Models;

namespace TransactionService.Repositories.DynamoDb
{
    public class DynamoDbTransactionRepository : ITransactionRepository
    {
        private readonly string _userId;
        private readonly DynamoDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Transaction";

        public DynamoDbTransactionRepository(IAmazonDynamoDB dbClient, CurrentUserContext currentUserContext, IMapper mapper)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _mapper = mapper;

            _userId = currentUserContext.UserId;
            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }
        
        public async Task<List<Transaction>> GetTransactions(DateRange dateRange, ITransactionSpecification spec)
        {
            var start = dateRange.Start;
            var end = dateRange.End;

            var transactions = await _dbContext.QueryAsync<DynamoDbTransaction>($"{_userId}{HashKeySuffix}",
                QueryOperator.Between, new[]
                {
                    $"{start:O}", $"{end:O}"
                }, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                    IndexName = "TransactionTimestampIndex"
                }).GetRemainingAsync();

            var filteredTransactions = transactions.Where(transaction => spec.IsSatisfied(transaction)).ToList();

            return _mapper.Map<List<DynamoDbTransaction>, List<Transaction>>(filteredTransactions);
        }

        public async Task StoreTransaction(Transaction newTransaction)
        {
            var newDynamoDbTransaction = _mapper.Map<Transaction, DynamoDbTransaction>(newTransaction);

            newDynamoDbTransaction.UserId = $"{_userId}{HashKeySuffix}";
            await _dbContext.SaveAsync(newDynamoDbTransaction, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }

        public async Task PutTransaction(Transaction newDynamoDbTransaction)
        {
            await StoreTransaction(newDynamoDbTransaction);
        }

        public async Task DeleteTransaction(string transactionId)
        {
            await _dbContext.DeleteAsync<DynamoDbTransaction>($"{_userId}{HashKeySuffix}", transactionId,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }
    }
}