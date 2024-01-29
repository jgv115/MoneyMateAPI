using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TransactionService.Middleware;

namespace TransactionService.Repositories.CockroachDb;

public record TransactionTrendPeriod
{
    public string PeriodStart { get; set; }

    public string PeriodEnd { get; set; }

    // Category, PayerPayee, 
    public string Identifier { get; set; }
    public decimal Amount { get; set; }
}

public class CockroachDbTransactionTrendsRepository
{
    private readonly DapperContext _context;
    private readonly CurrentUserContext _userContext;

    public CockroachDbTransactionTrendsRepository(DapperContext context, CurrentUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<IEnumerable<TransactionTrendPeriod>> GetTrendByCategory()
    {
        var query =
            @"
                SELECT DATE_TRUNC('MONTH',
                                  TIMEZONE('Australia/Melbourne', transaction_timestamp))                   as period_start,
                       DATE_TRUNC('MONTH', TIMEZONE('Australia/Melbourne', transaction_timestamp)) +
                       interval '1 month'                                                                   as period_end,
                       c.name                                                                               as identifier,
                       SUM(amount) FILTER ( WHERE transaction.transaction_type_id =
                                                  (SELECT id FROM transactiontype where name = 'expense') ) as expense_amount,
                       SUM(amount) FILTER ( WHERE transaction.transaction_type_id =
                                                  (SELECT id FROM transactiontype where name = 'income'))   as income_amount
                FROM transaction
                         LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id
                         LEFT JOIN category c on sc.category_id = c.id
                WHERE transaction_timestamp AT TIME ZONE 'Australia/Melbourne' > '2023-01-01'
                  AND transaction_timestamp AT TIME ZONE 'Australia/Melbourne' < '2024-02-01'
                  AND transaction.profile_id = '7e359a81-94cc-4b8a-bdf1-c8c795b79d34'
                GROUP BY period_start, period_end, identifier
                ORDER BY period_start DESC, expense_amount DESC";

        using (var connection = _context.CreateConnection())
        {
            var test = await connection.QueryAsync<TransactionTrendPeriod>(query);
            return test;
        }
    }
}