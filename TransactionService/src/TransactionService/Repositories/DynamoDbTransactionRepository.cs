using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public class DynamoDbTransactionRepository : ITransactionRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Transaction";

        public DynamoDbTransactionRepository(IAmazonDynamoDB dbClient)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync(string userId, DateTime start, DateTime end)
        {
            return await _dbContext.QueryAsync<Transaction>($"{userId}{HashKeySuffix}", QueryOperator.Between, new[]
            {
                $"{start:O}", $"{end:O}"
            }, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName,
                IndexName = "TransactionTimestampIndex"
            }).GetRemainingAsync();
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync(string userId, DateTime start,
            DateTime end, string transactionType)
        {
            return await _dbContext.QueryAsync<Transaction>($"{userId}{HashKeySuffix}", QueryOperator.Between, new[]
            {
                $"{start:O}", $"{end:O}"
            }, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName,
                IndexName = "TransactionTimestampIndex",
                QueryFilter = new List<ScanCondition>
                {
                    {
                        new("TransactionType", ScanOperator.BeginsWith, transactionType)
                    }
                }
            }).GetRemainingAsync();
        }

        public async Task<List<Transaction>> GetAllTransactionsByCategoryAsync(string userId, string categoryName,
            DateTime start, DateTime end)
        {
            return await _dbContext.QueryAsync<Transaction>($"{userId}{HashKeySuffix}", QueryOperator.Between, new[]
            {
                $"{start:O}", $"{end:O}"
            }, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName,
                IndexName = "TransactionTimestampIndex",
                QueryFilter = new List<ScanCondition>
                {
                    {
                        new("Category", ScanOperator.Equal, categoryName)
                    }
                }
            }).GetRemainingAsync(); 
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

        public async Task DeleteTransaction(string userId, string transactionId)
        {
            await _dbContext.DeleteAsync<Transaction>($"{userId}{HashKeySuffix}", transactionId,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }
    }
}