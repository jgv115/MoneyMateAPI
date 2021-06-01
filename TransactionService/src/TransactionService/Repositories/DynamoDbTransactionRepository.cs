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
        private readonly string _tableName;

        private const string TransactionSuffix = "#Transaction";
        
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
            return await _dbContext.QueryAsync<Transaction>($"{userId}{TransactionSuffix}", QueryOperator.Between, new[]
            {
                $"{start:O}", $"{end:O}"
            }, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName,
                IndexName = "TransactionTimestampIndex"
            }).GetRemainingAsync();
        }

        public async Task StoreTransaction(Transaction newTransaction)
        {
            newTransaction.UserId += TransactionSuffix;
            await _dbContext.SaveAsync(newTransaction, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }

        public async Task PutTransaction(Transaction newTransaction)
        {
            await StoreTransaction(newTransaction);
        }
    }
}