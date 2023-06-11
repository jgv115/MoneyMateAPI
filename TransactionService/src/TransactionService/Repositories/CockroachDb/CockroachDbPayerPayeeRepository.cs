using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Npgsql;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.CockroachDb
{
    public static class PayerPayeeDapperHelpers
    {
        public static async Task<IEnumerable<PayerPayee>> QueryAndBuildPayerPayees(IDbConnection connection,
            string query, object parameters = null)
        {
            var payerPayeeDictionary = new Dictionary<Guid, PayerPayee>();

            var payerPayees =
                await connection.QueryAsync<PayerPayee, PayerPayeeType, PayerPayeeExternalLinkType, PayerPayee>(query,
                    (payerPayee, payerPayeeType, payerPayeeExternalLinkType) =>
                    {
                        PayerPayee accumulatedPayerPayee;

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

        private async Task<IEnumerable<Domain.Models.PayerPayee>> QueryPayerPayees(string query, string payerPayeeType,
            string payerPayeeId)
        {
            using (var connection = _context.CreateConnection())
            {
                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new {user_identifier = _userContext.UserId, payerPayeeType, payerPayeeId});

                return _mapper.Map<IEnumerable<PayerPayee>, IEnumerable<Domain.Models.PayerPayee>>(payerPayees);
            }
        }

        private Task<IEnumerable<Domain.Models.PayerPayee>> QueryPayerPayees(string query, string payerPayeeType)
        {
            return QueryPayerPayees(query, payerPayeeType, "");
        }

        private Task<IEnumerable<Domain.Models.PayerPayee>> GetPayersPayees(string payerPayeeType,
            PaginationSpec paginationSpec)
        {
            var query =
                @"
                SELECT payerpayee.id,
                       u.id           as userId,
                       payerpayee.name             as name,
                       payerpayee.external_link_id as externalLinkId,
                       ppt.id,
                       ppt.name                    as name,
                       pext.id,
                       pext.name                   as name
                FROM payerpayee
                         JOIN users u on payerpayee.user_id = u.id
                         LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                         LEFT JOIN payerpayeeexternallinktype pext
                                   on payerpayee.external_link_type_id = pext.id
                WHERE u.user_identifier = @user_identifier and ppt.name = @payerPayeeType
                ";

            return QueryPayerPayees(query, payerPayeeType);
        }

        private async Task<Domain.Models.PayerPayee> GetPayerPayee(string payerPayeeId, string payerPayeeType)
        {
            var query =
                @"
                SELECT payerpayee.id,
                       u.id           as userId,
                       payerpayee.name             as name,
                       payerpayee.external_link_id as externalLinkId,
                       ppt.id,
                       ppt.name                    as name,
                       pext.id,
                       pext.name                   as name
                FROM payerpayee
                         JOIN users u on payerpayee.user_id = u.id
                         LEFT JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                         LEFT JOIN payerpayeeexternallinktype pext
                                   on payerpayee.external_link_type_id = pext.id
                WHERE 
                    u.user_identifier = @user_identifier 
                    AND ppt.name = @payerPayeeType
                    AND payerpayee.id = @payerPayeeId
                ";

            var payerPayees = await QueryPayerPayees(query, payerPayeeType, payerPayeeId);
            return payerPayees.FirstOrDefault((Domain.Models.PayerPayee) null);
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayers(PaginationSpec paginationSpec)
            => GetPayersPayees("Payer", paginationSpec);


        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayees(PaginationSpec paginationSpec)
            => GetPayersPayees("Payee", paginationSpec);


        public Task<Domain.Models.PayerPayee> GetPayer(Guid payerPayeeId)
            => GetPayerPayee(payerPayeeId.ToString(), "Payer");

        public Task<Domain.Models.PayerPayee> GetPayee(Guid payerPayeeId)
            => GetPayerPayee(payerPayeeId.ToString(), "Payee");

        private async Task CreatePayerPayee(Domain.Models.PayerPayee newPayerPayee, string payerPayeeType)
        {
            using (var connection = _context.CreateConnection())
            {
                var createPayerPayeeQuery =
                    @"
                        WITH ins (payerPayeeId, user_identifier, payerPayeeName, payerPayeeType, externalLinkType, externalId)
                                 AS (VALUES (@payerPayeeId, @user_identifier, @payerPayeeName, @payerPayeeType, @externalLinkId, @externalId))
                        INSERT
                        INTO payerpayee (id, user_id, name, payerPayeeType_id, external_link_type_id, external_link_id)
                        SELECT ins.payerPayeeId,
                               u.id,
                               ins.payerPayeeName,
                               p.id,
                               p2.id,
                               ins.externalId
                        FROM ins
                                 JOIN payerpayeetype p ON p.name = ins.payerPayeeType
                                 JOIN payerpayeeexternallinktype p2 on p2.name = ins.externalLinkType
                                 JOIN users u ON u.user_identifier = ins.user_identifier                  
                    ";

                try
                {
                    await connection.ExecuteAsync(createPayerPayeeQuery, new
                    {
                        payerPayeeId = Guid.Parse(newPayerPayee.PayerPayeeId),
                        user_identifier = _userContext.UserId,
                        payerPayeeName = newPayerPayee.PayerPayeeName,
                        payerPayeeType,
                        externalLinkId = string.IsNullOrEmpty(newPayerPayee.ExternalId) ? "Custom" : "Google",
                        externalId = newPayerPayee.ExternalId ?? ""
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

        public Task CreatePayer(Domain.Models.PayerPayee newPayerPayee)
            => CreatePayerPayee(newPayerPayee, "Payer");

        public Task CreatePayee(Domain.Models.PayerPayee newPayerPayee)
            => CreatePayerPayee(newPayerPayee, "Payee");

        public Task<IEnumerable<Domain.Models.PayerPayee>> FindPayer(string searchQuery)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> FindPayee(string searchQuery)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> AutocompletePayer(string autocompleteQuery)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> AutocompletePayee(string autocompleteQuery)
        {
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