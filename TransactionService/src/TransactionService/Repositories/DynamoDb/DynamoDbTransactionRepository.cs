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
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.DynamoDb
{
    public class DynamoDbTransactionRepository : ITransactionRepository
    {
        private readonly string _userId;
        private readonly DynamoDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Transaction";

        public DynamoDbTransactionRepository(IAmazonDynamoDB dbClient, CurrentUserContext currentUserContext,
            IMapper mapper, DynamoDbRepositoryConfig config)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _mapper = mapper;

            _userId = currentUserContext.UserId;
            _dbContext = new DynamoDBContext(dbClient);
            _tableName = config.TableName;
        }

        public async Task<Transaction> GetTransactionById(string transactionId)
        {
            var transaction =
                await _dbContext.LoadAsync<DynamoDbTransaction>($"{_userId}{HashKeySuffix}", transactionId,
                    new DynamoDBOperationConfig
                    {
                        OverrideTableName = _tableName
                    });

            if (transaction == null)
                throw new RepositoryItemDoesNotExist($"Transaction with ID: {transactionId} could not be found");

            return _mapper.Map<DynamoDbTransaction, Transaction>(transaction);
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