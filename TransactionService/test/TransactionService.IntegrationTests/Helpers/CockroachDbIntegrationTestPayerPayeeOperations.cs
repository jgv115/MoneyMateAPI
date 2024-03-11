using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using TransactionService.Domain.Models;
using TransactionService.Repositories.CockroachDb;

namespace TransactionService.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestPayerPayeeOperations
{
    private readonly DapperContext _dapperContext;
    private readonly Guid _testUserId;
    private IMapper _mapper;


    public CockroachDbIntegrationTestPayerPayeeOperations(
        DapperContext dapperContext,
        Guid testUserId,
        IMapper mapper
    )
    {
        _dapperContext = dapperContext;
        _testUserId = testUserId;
        _mapper = mapper;
    }

    public async Task<Guid> WritePayerPayeeIntoDb(PayerPayee payerPayee, string payerPayeeType)
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var createPayerPayeeQuery =
                @"
                    WITH ins (payerPayeeId, userId, payerPayeeName, payerPayeeType, externalLinkType, externalId, profileId)
                             AS (VALUES (@payerPayeeId, @userId, @payerPayeeName, @payerPayeeType, @externalLinkId, @externalId, @profileId)),
                         e AS (
                             INSERT
                                 INTO payerpayee (id, user_id, name, payerPayeeType_id, external_link_type_id, external_link_id, profile_id)
                                    SELECT ins.payerPayeeId,
                                           ins.userId,
                                           ins.payerPayeeName,
                                           p.id,
                                           p2.id,
                                           ins.externalId,
                                           ins.profileId
                                     FROM ins
                                              JOIN payerpayeetype p ON p.name = ins.payerPayeeType
                                              JOIN payerpayeeexternallinktype p2 on p2.name = ins.externalLinkType
                                     ON CONFLICT DO NOTHING
                                     RETURNING payerpayee.id)
                    SELECT *
                    FROM e
                    UNION
                    SELECT payerpayee.id
                    FROM payerpayee
                             JOIN users u on payerpayee.user_id = u.id
                             JOIN payerpayeetype ppt on payerpayee.payerpayeetype_id = ppt.id
                             JOIN payerpayeeexternallinktype ppelt on payerpayee.external_link_type_id = ppelt.id
                             JOIN ins ON u.id = ins.userId 
                                    AND payerpayee.name = ins.payerPayeeName AND ppt.name = ins.payerPayeeType
                                    AND ppelt.name = ins.externalLinkType 
                                    AND payerpayee.external_link_id = ins.externalId;                   
                    ";

            return await connection.QuerySingleAsync<Guid>(createPayerPayeeQuery, new
            {
                payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                userId = _testUserId,
                profileId = _testUserId,
                payerPayeeName = payerPayee.PayerPayeeName,
                payerPayeeType,
                externalLinkId = string.IsNullOrEmpty(payerPayee.ExternalId) ? "Custom" : "Google",
                externalId = string.IsNullOrEmpty(payerPayee.ExternalId) ? "" : payerPayee.ExternalId
            });
        }
    }

    private async Task WritePayerPayeesIntoDb(List<PayerPayee> payerPayees, string payerPayeeType)
    {
        foreach (var payerPayee in payerPayees)
        {
            await WritePayerPayeeIntoDb(payerPayee, payerPayeeType);
        }
    }

    public Task WritePayersIntoDb(List<PayerPayee> payers) => WritePayerPayeesIntoDb(payers, "payer");
    public Task WritePayeesIntoDb(List<PayerPayee> payers) => WritePayerPayeesIntoDb(payers, "payee");

    public async Task<List<PayerPayee>> RetrieveAllPayersPayees(string payerPayeeType)
    {
        using (var connection = _dapperContext.CreateConnection())
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
                    u.id = @user_id 
                    AND ppt.name = @payerPayeeType
                ";

            var payersPayees =
                await PayerPayeeDapperHelpers.QueryAndBuildPayerPayees(connection, query,
                    new {user_id = _testUserId, payerPayeeType});

            return _mapper.Map<List<TransactionService.Repositories.CockroachDb.Entities.PayerPayee>, List<PayerPayee>>(
                payersPayees.ToList());
        }
    }
}