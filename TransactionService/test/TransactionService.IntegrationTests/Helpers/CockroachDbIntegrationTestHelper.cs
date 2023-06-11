using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Profiles;

namespace TransactionService.IntegrationTests.Helpers;

internal class TransactionTypeIds
{
    public Guid Expense { get; init; }
    public Guid Income { get; init; }

    public TransactionTypeIds(Guid expenseId, Guid incomeId)
    {
        Expense = expenseId;
        Income = incomeId;
    }
}

public class CockroachDbIntegrationTestHelper
{
    public DapperContext DapperContext { get; init; }
    private Guid TestUserId { get; set; }
    private TransactionTypeIds TransactionTypeIds { get; set; }
    private IMapper _mapper { get; init; }

    public CockroachDbIntegrationTestHelper()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.dev.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        var cockroachDbConnectionString = config.GetSection("CockroachDb").GetValue<string>("ConnectionString") ??
                                          throw new ArgumentException(
                                              "Could not find CockroachDb connection string for CockroachDb helper");

        Console.WriteLine(">>>>");
        Console.WriteLine(cockroachDbConnectionString);

        DapperContext = new DapperContext(cockroachDbConnectionString);
        _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryEntityProfile>();
                cfg.AddProfile<PayerPayeeEntityProfile>();
            })
            .CreateMapper();
    }

    public async Task SeedRequiredData()
    {
        using (var connection = DapperContext.CreateConnection())
        {
            // Create a test user
            var insertUserQuery = @"INSERT INTO users (user_identifier) VALUES (@test_user_identifier) RETURNING id";
            TestUserId =
                await connection.QuerySingleAsync<Guid>(insertUserQuery,
                    new {test_user_identifier = "auth0|moneymatetest"});

            // Get transaction type ids
            var transactionTypeQuery = @"SELECT id from transactiontype WHERE name = 'Expense';
                                        SELECT id from transactiontype WHERE name = 'Income';";

            using (var transactionTypeIdsReader = connection.QueryMultiple(transactionTypeQuery))
            {
                var expenseId = transactionTypeIdsReader.ReadFirst<Guid>();
                var incomeId = transactionTypeIdsReader.ReadFirst<Guid>();
                if (expenseId == Guid.Empty || incomeId == Guid.Empty)
                    throw new ArgumentException(
                        "Transaction Type Ids are not valid when trying to seed CockroachDb data");
                TransactionTypeIds = new TransactionTypeIds(expenseId, incomeId);
            }
        }
    }

    public async Task ClearDbData()
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var query = @"TRUNCATE users, category, subcategory, payerpayee CASCADE";

            await connection.ExecuteAsync(query);
        }
    }

    public async Task WriteTransactionsIntoDb(List<Transaction> transactions)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var createTransactionQuery = @"
                INSERT INTO transaction (user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerpayee_id, notes)
                VALUES (@user_id, @timestamp, @transaction_type_id, @amount, @subcategory_id, @payerpayee_id, @notes)
            ";

            foreach (var transaction in transactions)
            {
                await connection.ExecuteAsync(createTransactionQuery, new
                {
                    user_id = TestUserId,
                    timestamp = transaction.TransactionTimestamp,
                    // TODO: this might not work
                    transaction_type_id = transaction.TransactionType == TransactionType.Expense.ToString()
                        ? TransactionTypeIds.Expense
                        : TransactionTypeIds.Income,
                    amount = transaction.Amount,
                    // TODO: add subcategoryId + payerPayeeId,
                    notes = transaction.Note
                });
            }
        }
    }

    private async Task WritePayerPayeesIntoDb(List<PayerPayee> payerPayees, string payerPayeeType)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var createPayerPayeeQuery =
                @"
                        WITH ins (payerPayeeId, userId, payerPayeeName, payerPayeeType, externalLinkType, externalId)
                                 AS (VALUES (@payerPayeeId, @userId, @payerPayeeName, @payerPayeeType, @externalLinkId, @externalId))
                        INSERT
                        INTO payerpayee (id, user_id, name, payerPayeeType_id, external_link_type_id, external_link_id)
                        SELECT ins.payerPayeeId,
                               ins.userId,
                               ins.payerPayeeName,
                               p.id,
                               p2.id,
                               ins.externalId
                        FROM ins
                                 JOIN payerpayeetype p ON p.name = ins.payerPayeeType
                                 JOIN payerpayeeexternallinktype p2 on p2.name = ins.externalLinkType;                     
                    ";
            foreach (var payerPayee in payerPayees)
            {
                await connection.ExecuteAsync(createPayerPayeeQuery, new
                {
                    payerPayeeId = Guid.Parse(payerPayee.PayerPayeeId),
                    userId = TestUserId,
                    payerPayeeName = payerPayee.PayerPayeeName,
                    payerPayeeType,
                    externalLinkId = string.IsNullOrEmpty(payerPayee.ExternalId) ? "Custom" : "Google",
                    externalId = payerPayee.ExternalId
                });
            }
        }
    }

    public Task WritePayersIntoDb(List<PayerPayee> payers) => WritePayerPayeesIntoDb(payers, "Payer");
    public Task WritePayeesIntoDb(List<PayerPayee> payers) => WritePayerPayeesIntoDb(payers, "Payee");

    public async Task<List<PayerPayee>> RetrieveAllPayersPayees(string payerPayeeType)
    {
        using (var connection = DapperContext.CreateConnection())
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
                    new {user_id = TestUserId, payerPayeeType = payerPayeeType});

            return _mapper.Map<List<TransactionService.Repositories.CockroachDb.Entities.PayerPayee>, List<PayerPayee>>(
                payersPayees.ToList());
        }
    }

    public async Task WriteCategoriesIntoDb(List<Category> categories)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            foreach (var category in categories)
            {
                // TODO: possibility here to check for duplicates
                var createCategoryQuery =
                    @"
                        INSERT INTO category (name, user_id, transaction_type_id) 
                        VALUES (@category_name, @user_id, @transaction_type_id)
                        RETURNING id
                    ";

                var categoryId = await connection.QuerySingleAsync<Guid>(createCategoryQuery,
                    new
                    {
                        category_name = category.CategoryName, user_id = TestUserId,
                        transaction_type_id = category.TransactionType == TransactionType.Expense
                            ? TransactionTypeIds.Expense
                            : TransactionTypeIds.Income
                    });


                var subcategoryQuery =
                    @"INSERT INTO subcategory (name, category_id) VALUES (@subcategory_name, @category_id)";
                await connection.ExecuteAsync(subcategoryQuery,
                    category.Subcategories.Select(s => new {subcategory_name = s, category_id = categoryId}));
            }
        }
    }

    public async Task<List<Category>> RetrieveAllCategories()
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var categoryQuery =
                @"SELECT * FROM category
                    LEFT JOIN subcategory s on category.id = s.category_id
                    WHERE category.user_id = @user_id
                    ORDER BY category.name, s.name";

            var categories =
                await CategoryDapperHelpers.QueryAndBuildCategories(connection, categoryQuery,
                    new {user_id = TestUserId});

            return _mapper.Map<List<TransactionService.Repositories.CockroachDb.Entities.Category>, List<Category>>(
                categories.ToList());
        }
    }
}