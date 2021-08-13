using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public class DynamoDbPayerPayeeRepository : IPayerPayeeRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#PayersPayees";

        private readonly Dictionary<string, string> _rangeKeySuffixes = new()
        {
            {"payee", "payee#"},
            {"payer", "payer#"}
        };

        public DynamoDbPayerPayeeRepository(IAmazonDynamoDB dbClient)
        {
            if (dbClient == null) throw new ArgumentNullException(nameof(dbClient));

            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        public async Task<IEnumerable<PayerPayee>> GetPayers(string userId)
        {
            var payers = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payer#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }
            ).GetRemainingAsync();

            payers.AsParallel().ForAll(payer => payer.UserId = payer.UserId.Split("#")[1]);
            return payers;
        }

        public async Task<IEnumerable<PayerPayee>> GetPayees(string userId)
        {
            var payees = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payee#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }
            ).GetRemainingAsync();
            payees.AsParallel().ForAll(payee => payee.UserId = payee.UserId.Split("#")[1]);

            return payees;
        }

        public async Task StorePayer(PayerPayee newPayerPayee)
        {
            newPayerPayee.UserId += HashKeySuffix;
            newPayerPayee.Name = $"{_rangeKeySuffixes["payer"]}{newPayerPayee.Name}";
            await _dbContext.SaveAsync(newPayerPayee, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }

        public Task StorePayee(PayerPayee newPayerPayee)
        {
            newPayerPayee.UserId += HashKeySuffix;
            newPayerPayee.Name = $"{_rangeKeySuffixes["payee"]}{newPayerPayee.Name}";
            throw new NotImplementedException();
        }

        public Task PutPayer(string userId)
        {
            throw new NotImplementedException();
        }

        public Task PutPayee(string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeletePayer(string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeletePayee(string userId)
        {
            throw new NotImplementedException();
        }
    }
}