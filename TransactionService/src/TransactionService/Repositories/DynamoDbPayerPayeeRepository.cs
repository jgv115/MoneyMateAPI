using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper.Internal;
using TransactionService.Helpers;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public class DynamoDbPayerPayeeRepository : IPayerPayeeRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#PayersPayees";
        private const string PayerPayeeNameIndex = "PayerPayeeNameIndex";

        private readonly Dictionary<string, string> _rangeKeyPrefixes = new()
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

        private string extractRangeKeyData(string rangeKey) => rangeKey.Split("#")[1];

        public async Task<IEnumerable<PayerPayee>> GetPayers(string userId)
        {
            var payers = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payer#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }
            ).GetRemainingAsync();

            payers.AsParallel().ForAll(payer => payer.PayerPayeeId = extractRangeKeyData(payer.PayerPayeeId));
            return payers;
        }

        public async Task<IEnumerable<PayerPayee>> GetPayees(string userId)
        {
            var payees = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payee#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                }
            ).GetRemainingAsync();
            payees.AsParallel().ForAll(payee => payee.PayerPayeeId = extractRangeKeyData(payee.PayerPayeeId));

            return payees;
        }

        public async Task<PayerPayee> GetPayer(string userId, Guid payerPayeeId)
        {
            var payer = await _dbContext.LoadAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payer"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            payer.PayerPayeeId = extractRangeKeyData(payer.PayerPayeeId);
            return payer;
        }

        public async Task<PayerPayee> GetPayee(string userId, Guid payerPayeeId)
        {
            var payee = await _dbContext.LoadAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payee"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            payee.PayerPayeeId = extractRangeKeyData(payee.PayerPayeeId);
            return payee;
        }

        private async Task<IEnumerable<PayerPayee>> QueryPayerPayeeByName(string userId, string payerOrPayee,
            IEnumerable<string> searchQueries)
        {
            var payerPayeeResults = new ConcurrentBag<PayerPayee>();
            var tasks = searchQueries.Select(async searchQuery =>
            {
                var results = await _dbContext.QueryAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                    QueryOperator.BeginsWith, new[] {searchQuery}, new DynamoDBOperationConfig
                    {
                        OverrideTableName = _tableName,
                        IndexName = PayerPayeeNameIndex,
                        QueryFilter = new List<ScanCondition>
                        {
                            new("PayerPayeeId", ScanOperator.BeginsWith, $"{payerOrPayee}#")
                        }
                    }
                ).GetRemainingAsync();
                
                results.ForAll(payerPayee =>
                {
                    payerPayee.PayerPayeeId = extractRangeKeyData(payerPayee.PayerPayeeId);
                    payerPayeeResults.Add(payerPayee);
                });
            });

            await Task.WhenAll(tasks);

            return payerPayeeResults;
        }

        private async Task<IEnumerable<PayerPayee>> FindPayerPayee(string userId, string payerOrPayee,
            string searchQuery)
        {
            var searchNames = StringHelpers.GenerateNGrams(searchQuery, multiCase: true);
            return await QueryPayerPayeeByName(userId, payerOrPayee, searchNames);
        }

        private async Task<IEnumerable<PayerPayee>> AutocompletePayerPayee(string userId, string payerOrPayee,
            string searchQuery)
        {
            var searchNames = new List<string>
            {
                searchQuery.CapitaliseFirstLetter(),
                searchQuery.LowercaseFirstLetter()
            };

            return await QueryPayerPayeeByName(userId, payerOrPayee, searchNames);
        }

        public Task<IEnumerable<PayerPayee>> FindPayer(string userId, string payerName)
        {
            return FindPayerPayee(userId, "payer", payerName);
        }

        public Task<IEnumerable<PayerPayee>> FindPayee(string userId, string payeeName)
        {
            return FindPayerPayee(userId, "payee", payeeName);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string userId, string autocompleteQuery)
        {
            return AutocompletePayerPayee(userId, "payer", autocompleteQuery);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string userId, string autocompleteQuery)
        {
            return AutocompletePayerPayee(userId, "payer", autocompleteQuery);
        }

        private async Task StorePayerPayee(PayerPayee newPayerPayee, string payerOrPayee)
        {
            if (payerOrPayee is "payer" or "payee")
            {
                newPayerPayee.UserId += HashKeySuffix;
                newPayerPayee.PayerPayeeId = $"{_rangeKeyPrefixes[payerOrPayee]}{newPayerPayee.PayerPayeeId}";
                await _dbContext.SaveAsync(newPayerPayee, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            }
            else
            {
                throw new Exception("payerPayee input not valid");
            }
        }

        public async Task CreatePayer(PayerPayee newPayerPayee)
        {
            await StorePayerPayee(newPayerPayee, "payer");
        }

        public async Task CreatePayee(PayerPayee newPayerPayee)
        {
            await StorePayerPayee(newPayerPayee, "payee");
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