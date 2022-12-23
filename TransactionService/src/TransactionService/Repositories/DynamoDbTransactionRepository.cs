using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;

namespace TransactionService.Repositories
{
    public class DynamoDbTransactionRepository : ITransactionRepository
    {
        private readonly string _userId;
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Transaction";

        public DynamoDbTransactionRepository(IAmazonDynamoDB dbClient, CurrentUserContext currentUserContext)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _userId = currentUserContext.UserId;
            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        private string ExtractPublicFacingUserId(string input) => input.Split("#")[0];

        public async Task<List<Transaction>> GetTransactions(DateRange dateRange, ITransactionSpecification spec)
        {
            var start = dateRange.Start;
            var end = dateRange.End;

            var transactions = await _dbContext.QueryAsync<Transaction>($"{_userId}{HashKeySuffix}",
                QueryOperator.Between, new[]
                {
                    $"{start:O}", $"{end:O}"
                }, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                    IndexName = "TransactionTimestampIndex"
                }).GetRemainingAsync();

            return transactions.Where(transaction => spec.IsSatisfied(transaction)).Select(transaction =>
            {
                transaction.UserId = ExtractPublicFacingUserId(transaction.UserId);
                return transaction;
            }).ToList();
        }

        public async Task StoreTransaction(Transaction newTransaction)
        {
            newTransaction.UserId += HashKeySuffix;
            await _dbContext.SaveAsync(newTransaction, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }

        public async Task PutTransaction(Transaction newTransaction)
        {
            await StoreTransaction(newTransaction);
        }

        public async Task DeleteTransaction(string transactionId)
        {
            await _dbContext.DeleteAsync<Transaction>($"{_userId}{HashKeySuffix}", transactionId,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }
    }
}