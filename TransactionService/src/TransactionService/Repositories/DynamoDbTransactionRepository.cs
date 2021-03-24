using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public class DynamoDbTransactionRepository : ITransactionRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly DynamoDBOperationConfig _dbOperationConfig;

        public DynamoDbTransactionRepository(IAmazonDynamoDB dbClient)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }
            
            _dbContext = new DynamoDBContext(dbClient);
            _dbOperationConfig = new DynamoDBOperationConfig
            {
                OverrideTableName =
                    $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
            };
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync(string userId, DateTime start, DateTime end)
        {
            return await _dbContext.QueryAsync<Transaction>(userId, QueryOperator.Between, new[]
            {
                $"{start:O}", $"{end:O}"
            }, _dbOperationConfig).GetRemainingAsync();
        }

        public async Task StoreTransaction(Transaction newTransaction)
        {
            await _dbContext.SaveAsync(newTransaction, _dbOperationConfig);
        }
    }
}