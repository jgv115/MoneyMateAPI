using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.DynamoDb.Models;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.DynamoDb
{
    public class DynamoDbPayerPayeeRepository : IPayerPayeeRepository
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly string _userId;
        private readonly string _tableName;
        private readonly IMapper _mapper;

        private const string HashKeySuffix = "#PayersPayees";
        private const string PayerPayeeNameIndex = "PayerPayeeNameIndex";

        private readonly Dictionary<string, string> _rangeKeyPrefixes = new()
        {
            {"payee", "payee#"},
            {"payer", "payer#"}
        };

        public DynamoDbPayerPayeeRepository(DynamoDbRepositoryConfig config, IDynamoDBContext dbContext,
            CurrentUserContext userContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _userId = userContext.UserId;
            _mapper = mapper;
            _tableName = config.TableName;
        }


        private IEnumerable<DynamoDbPayerPayee> PaginateResults(List<DynamoDbPayerPayee> results,
            PaginationSpec paginationSpec)
        {
            var paginatedPayees = results
                .Skip(paginationSpec.Offset)
                .Take(paginationSpec.Limit);

            return paginatedPayees;
        }

        public async Task<IEnumerable<PayerPayee>> GetPayers(PaginationSpec paginationSpec)
        {
            var payers = await _dbContext.QueryAsync<DynamoDbPayerPayee>(
                $"{_userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payer#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }
            ).GetRemainingAsync();

            var paginatedPayers = PaginateResults(payers, paginationSpec);
            return _mapper.Map<IEnumerable<DynamoDbPayerPayee>, IEnumerable<PayerPayee>>(paginatedPayers);
        }

        public async Task<IEnumerable<PayerPayee>> GetPayees(PaginationSpec paginationSpec)
        {
            var payees = await _dbContext.QueryAsync<DynamoDbPayerPayee>(
                $"{_userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {"payee#"}, new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                }
            ).GetRemainingAsync();

            var paginatedPayees = PaginateResults(payees, paginationSpec);
            return _mapper.Map<IEnumerable<DynamoDbPayerPayee>, IEnumerable<PayerPayee>>(paginatedPayees);
        }

        public async Task<PayerPayee> GetPayer(Guid payerPayeeId)
        {
            var payer = await _dbContext.LoadAsync<DynamoDbPayerPayee>($"{_userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payer"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            return _mapper.Map<DynamoDbPayerPayee, PayerPayee>(payer);
        }

        public async Task<PayerPayee> GetPayee(Guid payerPayeeId)
        {
            var payee = await _dbContext.LoadAsync<DynamoDbPayerPayee>($"{_userId}{HashKeySuffix}",
                $"{_rangeKeyPrefixes["payee"]}{payerPayeeId}", new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName,
                });
            return _mapper.Map<DynamoDbPayerPayee, PayerPayee>(payee);
        }

        private async Task<DynamoDbPayerPayee> QueryPayerPayee(string payerOrPayee, string name,
            string externalId)
        {
            var results = await _dbContext.QueryAsync<DynamoDbPayerPayee>($"{_userId}{HashKeySuffix}",
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

        private async Task<IEnumerable<PayerPayee>> QueryPayerPayee(string payerOrPayee,
            IEnumerable<string> nameSearchQueries)
        {
            var payerPayeeResults = new ConcurrentBag<DynamoDbPayerPayee>();
            var tasks = nameSearchQueries.Select(async searchQuery =>
            {
                var results = await _dbContext.QueryAsync<DynamoDbPayerPayee>($"{_userId}{HashKeySuffix}",
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

                foreach (var dynamoDbPayerPayee in results)
                {
                    payerPayeeResults.Add(dynamoDbPayerPayee);
                }
            });

            await Task.WhenAll(tasks);

            var distinctPayerPayees = payerPayeeResults.DistinctBy(payerpayee => payerpayee.PayerPayeeId);

            return _mapper.Map<IEnumerable<DynamoDbPayerPayee>, IEnumerable<PayerPayee>>(distinctPayerPayees);
        }

        private async Task<IEnumerable<PayerPayee>> FindPayerPayee(string payerOrPayee,
            string searchQuery)
        {
            var searchNames = StringHelpers.GenerateNGrams(searchQuery, multiCase: true);
            return await QueryPayerPayee(payerOrPayee, searchNames);
        }

        private async Task<IEnumerable<PayerPayee>> AutocompletePayerPayee(string payerOrPayee,
            string searchQuery)
        {
            var searchNames = StringHelpers.GenerateCapitilisationCombinations(searchQuery);

            return await QueryPayerPayee(payerOrPayee, searchNames);
        }

        public Task<IEnumerable<PayerPayee>> FindPayer(string payerName)
        {
            return FindPayerPayee("payer", payerName);
        }

        public Task<IEnumerable<PayerPayee>> FindPayee(string payeeName)
        {
            return FindPayerPayee("payee", payeeName);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayer(string autocompleteQuery)
        {
            return AutocompletePayerPayee("payer", autocompleteQuery);
        }

        public Task<IEnumerable<PayerPayee>> AutocompletePayee(string autocompleteQuery)
        {
            return AutocompletePayerPayee("payee", autocompleteQuery);
        }

        private async Task StorePayerPayee(PayerPayee newPayerPayee, string payerOrPayee)
        {
            if (payerOrPayee is "payer" or "payee")
            {
                var foundPayerPayee = await QueryPayerPayee(payerOrPayee,
                    newPayerPayee.PayerPayeeName,
                    newPayerPayee.ExternalId);

                if (foundPayerPayee is not null)
                {
                    throw new RepositoryItemExistsException(
                        $"{payerOrPayee} with name {newPayerPayee.PayerPayeeName} and externalId {newPayerPayee.ExternalId} already exists");
                }

                var newDynamoDbPayerPayee = _mapper.Map<PayerPayee, DynamoDbPayerPayee>(newPayerPayee);

                newDynamoDbPayerPayee.UserId = $"{_userId}{HashKeySuffix}";
                newDynamoDbPayerPayee.PayerPayeeId = $"{_rangeKeyPrefixes[payerOrPayee]}{newPayerPayee.PayerPayeeId}";
                await _dbContext.SaveAsync(newDynamoDbPayerPayee, new DynamoDBOperationConfig
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

        public async Task CreatePayee(PayerPayee newDynamoDbPayerPayee)
        {
            await StorePayerPayee(newDynamoDbPayerPayee, "payee");
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