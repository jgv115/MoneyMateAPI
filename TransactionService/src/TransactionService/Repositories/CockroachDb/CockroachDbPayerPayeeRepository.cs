using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;

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

        private async Task<IEnumerable<Domain.Models.PayerPayee>> GetPayersPayees(string payerPayeeType,
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

            using (var connection = _context.CreateConnection())
            {
                var payerPayees = await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new {user_identifier = _userContext.UserId, payerPayeeType});

                return _mapper.Map<IEnumerable<PayerPayee>, IEnumerable<Domain.Models.PayerPayee>>(payerPayees);
            }
        }

        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayers(PaginationSpec paginationSpec)
            => GetPayersPayees("Payer", paginationSpec);


        public Task<IEnumerable<Domain.Models.PayerPayee>> GetPayees(PaginationSpec paginationSpec)
            => GetPayersPayees("Payee", paginationSpec);


        public Task<Domain.Models.PayerPayee> GetPayer(Guid payerPayeeId)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Models.PayerPayee> GetPayee(Guid payerPayeeId)
        {
            throw new NotImplementedException();
        }

        public Task CreatePayer(Domain.Models.PayerPayee newPayerPayee)
        {
            throw new NotImplementedException();
        }

        public Task CreatePayee(Domain.Models.PayerPayee newPayerPayee)
        {
            throw new NotImplementedException();
        }

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