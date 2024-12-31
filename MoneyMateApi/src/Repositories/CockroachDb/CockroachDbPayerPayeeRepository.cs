using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using MoneyMateApi.Domain.Services.PayerPayees;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using MoneyMateApi.Repositories.Exceptions;
using Npgsql;

namespace MoneyMateApi.Repositories.CockroachDb
{
    public static class PayerPayeeDapperHelpers
    {
        public static async Task<IEnumerable<PayerPayee>> QueryAndBuildPayerPayees(IDbConnection connection,
            string query, object? parameters = null)
        {
            var payerPayeeDictionary = new Dictionary<Guid, PayerPayee>();

            var payerPayees =
                await connection.QueryAsync<PayerPayee, PayerPayeeType?, PayerPayeeExternalLinkType?, PayerPayee>(query,
                    (payerPayee, payerPayeeType, payerPayeeExternalLinkType) =>
                    {
                        PayerPayee? accumulatedPayerPayee;

                        if (!payerPayeeDictionary.TryGetValue(payerPayee.Id, out accumulatedPayerPayee))
                        {
                            accumulatedPayerPayee = payerPayee;
                            payerPayeeDictionary.Add(accumulatedPayerPayee.Id, accumulatedPayerPayee);
                        }

                        if (payerPayeeType is not null)
                            accumulatedPayerPayee.PayerPayeeType = payerPayeeType;

                        if (payerPayeeExternalLinkType is not null)
                            accumulatedPayerPayee.PayerPayeeExternalLinkType = payerPayeeExternalLinkType;

                        return accumulatedPayerPayee;
                    }, parameters);

            return payerPayees.Distinct();
        }
    }

    internal class PayerPayeeSuggestionQueryBuilder
    {
        private Dictionary<string, object> SuggestionQueryParams { get; set; } = new();
        private string SuggestionQuery { get; set; } = "";

        public PayerPayeeSuggestionQueryBuilder(Dictionary<string, object> initialQueryParams)
        {
            SuggestionQueryParams = SuggestionQueryParams.Concat(initialQueryParams)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public PayerPayeeSuggestionQueryBuilder WithSuggestionParameters(
            IPayerPayeeSuggestionParameters suggestionParameters)
        {
            // TODO: might need to put a time limit in here in the future
            var baseQuery = @"
                        SELECT payerpayee.id,
                               payerpayee.name             as name,
                               payerpayee.external_link_id as externalLinkId,
                               ppt.id,
                               ppt.name                    as name,
                               pext.id,
                               pext.name                   as name
                        FROM groupedPayerPayees
                                LEFT JOIN payerpayee ON payerpayee.id = groupedPayerPayees.payerpayee_id
                                 LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                                 LEFT JOIN payerpayeeexternallinktype pext
                                           on payerpayee.external_link_type_id = pext.id";

            if (suggestionParameters is SubcategoryPayerPayeeSuggestionParameters parameters)
            {
                SuggestionQuery = @"
                                    WITH groupedPayerPayees AS (SELECT payerpayee_id, subcategory_id, count(1) as count
                                         FROM transaction
                                         WHERE profile_id = @profileId
                                         group by payerpayee_id, subcategory_id)" +
                                  baseQuery +
                                  " LEFT JOIN categories_and_subcategories cs on cs.subcategoryid = groupedPayerPayees.subcategory_id" +
                                  @" WHERE ppt.name = @payerPayeeType 
                                            and cs.subcategoryname = @subcategoryName 
                                            and cs.categoryname = @categoryName";

                SuggestionQueryParams.Add("categoryName", parameters.Category);
                SuggestionQueryParams.Add("subcategoryName", parameters.Subcategory);
            }
            else
            {
                SuggestionQuery = @"
                                    WITH groupedPayerPayees AS (SELECT payerpayee_id, subcategory_id, count(1) as count
                                         FROM transaction
                                         WHERE profile_id = @profileId
                                         group by payerpayee_id, subcategory_id)" +
                                  baseQuery +
                                  " WHERE ppt.name = @payerPayeeType";
            }

            SuggestionQuery = SuggestionQuery + " order by count desc LIMIT @limit;";
            return this;
        }

        public string BuildQuery() => SuggestionQuery;
        public object BuildQueryParams() => SuggestionQueryParams;
    }

    public class CockroachDbPayerPayeeRepository : IPayerPayeeRepository
    {
        private readonly DapperContext _context;
        private readonly IMapper _mapper;
        private readonly CurrentUserContext _userContext;

        public CockroachDbPayerPayeeRepository(DapperContext context, IMapper mapper, CurrentUserContext userContext)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
        }

        private async Task<IEnumerable<Domain.Models.PayerPayee>> GetPayersPayees(string payerPayeeType,
            PaginationSpec paginationSpec)
        {
            var query =
                @"
                SELECT payerpayee.id,
                       payerpayee.name             as name,
                       payerpayee.external_link_id as externalLinkId,
                       ppt.id,
                       ppt.name                    as name,
                       pext.id,
                       pext.name                   as name
                FROM payerpayee
                         LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                         LEFT JOIN payerpayeeexternallinktype pext
                                   on payerpayee.external_link_type_id = pext.id
                WHERE payerpayee.profile_id = @profile_id and ppt.name = @payerPayeeType
                LIMIT 20
                ";

            using (var connection = _context.CreateConnection())
            {
                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new { profile_id = _userContext.ProfileId, payerPayeeType });

                return _mapper.Map<IEnumerable<PayerPayee>, IEnumerable<Domain.Models.PayerPayee>>(payerPayees);
            }
        }

        private async Task<Domain.Models.PayerPayee> GetPayerPayee(string payerPayeeId, string payerPayeeType)
        {
            var query =
                @"
                SELECT payerpayee.id,
                       payerpayee.name             as name,
                       payerpayee.external_link_id as externalLinkId,
                       ppt.id,
                       ppt.name                    as name,
                       pext.id,
                       pext.name                   as name
                FROM payerpayee
                         LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                         LEFT JOIN payerpayeeexternallinktype pext
                                   on payerpayee.external_link_type_id = pext.id
                WHERE 
                    payerpayee.profile_id = @profile_id 
                    AND ppt.name = @payerPayeeType
                    AND payerpayee.id = @payerPayeeId
                ";

            using (var connection = _context.CreateConnection())
            {
                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new { profile_id = _userContext.ProfileId, payerPayeeType, payerPayeeId });

                return _mapper.Map<PayerPayee, Domain.Models.PayerPayee>(
                    payerPayees.FirstOrDefault((PayerPayee)null));
            }
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayers(PaginationSpec paginationSpec)
            => GetPayersPayees("payer", paginationSpec);


        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayees(PaginationSpec paginationSpec)
            => GetPayersPayees("payee", paginationSpec);


        public Task<Domain.Models.PayerPayee> GetPayer(Guid payerPayeeId)
            => GetPayerPayee(payerPayeeId.ToString(), "payer");

        public Task<Domain.Models.PayerPayee> GetPayee(Guid payerPayeeId)
            => GetPayerPayee(payerPayeeId.ToString(), "payee");

        private async Task InsertPayerPayee(Domain.Models.PayerPayee newPayerPayee,
            Constants.PayerPayeeType payerPayeeType,
            bool enforceUnique = true)
        {
            using (var connection = _context.CreateConnection())
            {
                var createPayerPayeeQuery =
                    @"
                        WITH ins (payerPayeeId, user_identifier, payerPayeeName, payerPayeeType, externalLinkType, externalId, profileId)
                                 AS (VALUES (@payerPayeeId, @user_identifier, @payerPayeeName, @payerPayeeType, @externalLinkId, @externalId, @profileId))"
                    +
                    (enforceUnique ? "INSERT" : "UPSERT") +
                    @" INTO payerpayee (id, user_id, name, payerPayeeType_id, external_link_type_id, external_link_id, profile_id)
                        SELECT ins.payerPayeeId,
                               u.id,
                               ins.payerPayeeName,
                               p.id,
                               p2.id,
                               ins.externalId,
                               ins.profileId
                        FROM ins
                                 JOIN payerpayeetype p ON p.name = ins.payerPayeeType
                                 JOIN payerpayeeexternallinktype p2 on p2.name = ins.externalLinkType
                                 JOIN users u ON u.user_identifier = ins.user_identifier                  
                    ";

                try
                {
                    var payerPayeeTypeString = ConvertPayerPayeeTypeEnumToString(payerPayeeType);
                    await connection.ExecuteAsync(createPayerPayeeQuery, new
                    {
                        payerPayeeId = Guid.Parse(newPayerPayee.PayerPayeeId),
                        user_identifier = _userContext.UserId,
                        payerPayeeName = newPayerPayee.PayerPayeeName,
                        payerPayeeType = payerPayeeTypeString,
                        externalLinkId = string.IsNullOrEmpty(newPayerPayee.ExternalId) ? "Custom" : "Google",
                        externalId = newPayerPayee.ExternalId ?? "",
                        profileId = _userContext.ProfileId
                    });
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == "23505")
                    {
                        throw new RepositoryItemExistsException("Exists");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public async Task CreatePayerOrPayee(Constants.PayerPayeeType payerPayeeType,
            Domain.Models.PayerPayee newPayerPayee)
        {
            await InsertPayerPayee(newPayerPayee, payerPayeeType);
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> FindPayer(string searchQuery)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> FindPayee(string searchQuery)
        {
            throw new NotImplementedException();
        }


        private async Task<IEnumerable<Domain.Models.PayerPayee>> AutocompletePayerPayee(string autocompleteQuery,
            string payerPayeeType)
        {
            var query =
                @"
                SELECT payerpayee.id,
                       payerpayee.name             as name,
                       payerpayee.external_link_id as externalLinkId,
                       ppt.id,
                       ppt.name                    as name,
                       pext.id,
                       pext.name                   as name
                FROM payerpayee
                         LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                         LEFT JOIN payerpayeeexternallinktype pext
                                   on payerpayee.external_link_type_id = pext.id
                WHERE 
                    payerpayee.profile_id = @profile_id 
                    AND ppt.name LIKE  @payerPayeeType
                    AND payerpayee.name ILIKE '%' || @payerPayeeName || '%'
                ";

            using (var connection = _context.CreateConnection())
            {
                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new
                    {
                        profile_id = _userContext.ProfileId,
                        payerPayeeType,
                        payerPayeeName = autocompleteQuery
                    });

                return _mapper.Map<IEnumerable<PayerPayee>, IEnumerable<Domain.Models.PayerPayee>>(payerPayees);
            }
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> AutocompletePayer(string autocompleteQuery)
            => AutocompletePayerPayee(autocompleteQuery, "payer");

        public Task<IEnumerable<Domain.Models.PayerPayee>> AutocompletePayee(string autocompleteQuery)
            => AutocompletePayerPayee(autocompleteQuery, "payee");

        public async Task<IEnumerable<Domain.Models.PayerPayee>> GetSuggestedPayersOrPayees(
            Constants.PayerPayeeType payerPayeeType, IPayerPayeeSuggestionParameters suggestionParameters,
            int limit = 20)
        {
            using (var connection = _context.CreateConnection())
            {
                var dapperQueryParams = new Dictionary<string, object>();
                dapperQueryParams.Add("profileId", _userContext.ProfileId);
                dapperQueryParams.Add("payerPayeeType", ConvertPayerPayeeTypeEnumToString(payerPayeeType));
                dapperQueryParams.Add("limit", limit);

                var suggestionQueryBuilder =
                    new PayerPayeeSuggestionQueryBuilder(dapperQueryParams).WithSuggestionParameters(
                        suggestionParameters);

                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(
                    connection,
                    suggestionQueryBuilder.BuildQuery(),
                    suggestionQueryBuilder.BuildQueryParams());
                return _mapper.Map<IEnumerable<PayerPayee>, IEnumerable<Domain.Models.PayerPayee>>(payerPayees);
            }
        }

        public Task PutPayerOrPayee(Constants.PayerPayeeType type, Domain.Models.PayerPayee newPayerPayee)
        {
            return InsertPayerPayee(newPayerPayee, type, enforceUnique: false);
        }

        public Task DeletePayer(string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeletePayee(string userId)
        {
            throw new NotImplementedException();
        }

        private string ConvertPayerPayeeTypeEnumToString(Constants.PayerPayeeType payerPayeeType)
        {
            return payerPayeeType switch
            {
                Constants.PayerPayeeType.Payer => "payer",
                Constants.PayerPayeeType.Payee => "payee",
                _ => throw new ArgumentOutOfRangeException(nameof(payerPayeeType), payerPayeeType,
                    "PayerPayeeType not supported in repository")
            };
        }
    }
}