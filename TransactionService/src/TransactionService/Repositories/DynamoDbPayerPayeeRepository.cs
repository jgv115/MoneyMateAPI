using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper.Internal;
using TransactionService.Domain.Models;
using TransactionService.Helpers;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories
{
    public class DynamoDbPayerPayeeRepository : IPayerPayeeRepository
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#PayersPayees";
        private const string PayerPayeeNameIndex = "PayerPayeeNameIndex";

        private readonly Dictionary<string, string> _rangeKeyPrefixes = new()
        {
            {"payee", "payee#"},
            {"payer", "payer#"}
        };

        public DynamoDbPayerPayeeRepository(DynamoDbRepositoryConfig config, IDynamoDBContext dbContext)
        {
            _dbContext = dbContext;
            _tableName = config.TableName;
        }

        private string ExtractRangeKeyData(string rangeKey) => rangeKey.Split("#")[1];

        private IEnumerable<PayerPayee> PaginateResults(List<PayerPayee> results, PaginationSpec paginationSpec)
        {
            var paginatedPayees = results
                .Skip(paginationSpec.Offset)
                .Take(paginationSpec.Limit)
                .Select(payerPayee =>
                {
                    payerPayee.PayerPayeeId = ExtractRangeKeyData(payerPayee.PayerPayeeId);
                    return payerPayee;
                });

            return paginatedPayees;
        }

        public async Task<IEnumerable<PayerPayee>> GetPayers(string userId, PaginationSpec paginationSpec)
        {
            var payers = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payer#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }
            ).GetRemainingAsync();

            return PaginateResults(payers, paginationSpec);

        }

        public async Task<IEnumerable<PayerPayee>> GetPayees(string userId, PaginationSpec paginationSpec)
        {
            var payees = await _dbContext.QueryAsync<PayerPayee>(
                $"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payee#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                }
            ).GetRemainingAsync();

            return PaginateResults(payees, paginationSpec);
        }

        public async Task<PayerPayee> GetPayer(string userId, Guid payerPayeeId)
        {
            var payer = await _dbContext.LoadAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payer"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            payer.PayerPayeeId = ExtractRangeKeyData(payer.PayerPayeeId);
            return payer;
        }

        public async Task<PayerPayee> GetPayee(string userId, Guid payerPayeeId)
        {
            var payee = await _dbContext.LoadAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payee"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            payee.PayerPayeeId = ExtractRangeKeyData(payee.PayerPayeeId);
            return payee;
        }

        private async Task<PayerPayee> QueryPayerPayee(string userId, string payerOrPayee, string name,
            string externalId)
        {
            var results = await _dbContext.QueryAsync<PayerPayee>($"{userId}{HashKeySuffix}",
                QueryOperator.Equal, new[] {name}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                    IndexName = PayerPayeeNameIndex,
                    QueryFilter = new List<ScanCondition>
                    {
                        new("PayerPayeeId", ScanOperator.BeginsWith, $"{payerOrPayee}#")
                    }
                }
            ).GetRemainingAsync();

            return results.Find(payerPayee => payerPayee.ExternalId == externalId);
        }

        private async Task<IEnumerable<PayerPayee>> QueryPayerPayee(string userId, string payerOrPayee,
            IEnumerable<string> nameSearchQueries)
        {
            var payerPayeeResults = new ConcurrentBag<PayerPayee>();
            var tasks = nameSearchQueries.Select(async searchQuery =>
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
                    payerPayee.PayerPayeeId = ExtractRangeKeyData(payerPayee.PayerPayeeId);
                    payerPayeeResults.Add(payerPayee);
                });
            });

            await Task.WhenAll(tasks);

            return payerPayeeResults.DistinctBy(payerpayee => payerpayee.PayerPayeeId);
        }

        private async Task<IEnumerable<PayerPayee>> FindPayerPayee(string userId, string payerOrPayee,
            string searchQuery)
        {
            var searchNames = StringHelpers.GenerateNGrams(searchQuery, multiCase: true);
            return await QueryPayerPayee(userId, payerOrPayee, searchNames);
        }

        private async Task<IEnumerable<PayerPayee>> AutocompletePayerPayee(string userId, string payerOrPayee,
            string searchQuery)
        {
            var searchNames = StringHelpers.GenerateCapitilisationCombinations(searchQuery);

            return await QueryPayerPayee(userId, payerOrPayee, searchNames);
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
            return AutocompletePayerPayee(userId, "payee", autocompleteQuery);
        }

        private async Task StorePayerPayee(PayerPayee newPayerPayee, string payerOrPayee)
        {
            if (payerOrPayee is "payer" or "payee")
            {
                var foundPayerPayee = await QueryPayerPayee(newPayerPayee.UserId, payerOrPayee,
                    newPayerPayee.PayerPayeeName,
                    newPayerPayee.ExternalId);

                if (foundPayerPayee is not null)
                {
                    throw new RepositoryItemExistsException(
                        $"{payerOrPayee} with name {newPayerPayee.PayerPayeeName} and externalId {newPayerPayee.ExternalId} already exists");
                }

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