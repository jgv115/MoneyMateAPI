using System;
using Dapper;
using TransactionService.Repositories.CockroachDb;

namespace TransactionService.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestTransactionTypeOperations
{
    private readonly DapperContext _dapperContext;

    public CockroachDbIntegrationTestTransactionTypeOperations(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }

    public TransactionTypeIds GetTransactionTypeIds()
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var transactionTypeQuery = @"SELECT id from transactiontype WHERE name = 'expense';
                                        SELECT id from transactiontype WHERE name = 'income';";

            using (var transactionTypeIdsReader = connection.QueryMultiple(transactionTypeQuery))
            {
                var expenseId = transactionTypeIdsReader.ReadFirst<Guid>();
                var incomeId = transactionTypeIdsReader.ReadFirst<Guid>();
                if (expenseId == Guid.Empty || incomeId == Guid.Empty)
                    throw new ArgumentException(
                        "Transaction Type Ids are not valid when trying to seed CockroachDb data");
                return new TransactionTypeIds(expenseId, incomeId);
            }
        }
    }
}