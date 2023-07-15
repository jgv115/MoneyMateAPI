using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
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

    // TODO: need to make this a bit easier to understand
    private string TestUserIdentifier { get; } = "auth0|moneymatetest";
    private TransactionTypeIds TransactionTypeIds { get; set; }
    private IMapper Mapper { get; init; }
    private CockroachDbTransactionRepository _transactionRepository { get; init; }

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

        DapperContext = new DapperContext(cockroachDbConnectionString);
        Mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryEntityProfile>();
                cfg.AddProfile<PayerPayeeEntityProfile>();
                cfg.AddProfile<TransactionEntityProfile>();
            })
            .CreateMapper();

        _transactionRepository = new CockroachDbTransactionRepository(DapperContext, Mapper, new CurrentUserContext
        {
            UserId = TestUserIdentifier
        });
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
            var transactionTypeQuery = @"SELECT id from transactiontype WHERE name = 'expense';
                                        SELECT id from transactiontype WHERE name = 'income';";

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
                INSERT INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerpayee_id, notes)
                VALUES (@transactionId, @userId, @timestamp, @transactionTypeId, @amount, @subcategoryId, @payerpayeeId, @notes)
            ";

            foreach (var transaction in transactions)
            {
                var parameters = new DynamicParameters();
                parameters.Add("transactionId", transaction.TransactionId);
                parameters.Add("userId", TestUserId);
                parameters.Add("timestamp", transaction.TransactionTimestamp);
                parameters.Add("transactionTypeId",
                    transaction.TransactionType == TransactionType.Expense.ToProperString()
                        ? TransactionTypeIds.Expense
                        : TransactionTypeIds.Income);
                parameters.Add("amount", transaction.Amount);
                parameters.Add("notes", transaction.Note);

                TransactionType parsedTransactionType;
                Enum.TryParse(transaction.TransactionType, out parsedTransactionType);
                var categoryId = await WriteCategoryIntoDb(connection, new Category
                {
                    TransactionType = parsedTransactionType,
                    CategoryName = transaction.Category
                });

                var subcategoryId = await WriteSubcategoryIntoDb(connection, categoryId, transaction.Subcategory);
                parameters.Add("subcategoryId", subcategoryId);

                if (string.IsNullOrEmpty(transaction.PayerPayeeId))
                    parameters.Add("payerPayeeId");
                else
                {
                    var payerPayeeId = await WritePayerPayeeIntoDb(
                        new PayerPayee
                        {
                            PayerPayeeId = transaction.PayerPayeeId,
                            PayerPayeeName = transaction.PayerPayeeName,
                            ExternalId = ""
                        }, transaction.TransactionType == TransactionType.Expense.ToProperString() ? "payee" : "payer");
                    parameters.Add("payerPayeeId", payerPayeeId);
                }

                await connection.ExecuteAsync(createTransactionQuery, parameters);
            }
        }
    }

    public async Task<List<Transaction>> GetAllTransactions()
    {
//         using (var connection = DapperContext.CreateConnection())
//         {
//             var query =
//                 @"SELECT transaction.id,
//                         u.id           as userId,
//                         transaction.transaction_timestamp as transactionTimestamp,
//                         transaction.amount,
//                         transaction.notes as note,
//
//                         tt.id,
//                         tt.name as name,
//                         
//                         c.id,
//                         c.name as name,
//                         
//                         sc.id,
//                         sc.name as name,
//                         
//                         pp.id,
//                         pp.name             as name,
//                         pp.external_link_id as externalLinkId
//                  FROM transaction
//                          JOIN users u on transaction.user_id = u.id
//                          LEFT JOIN transactiontype tt on transaction.transaction_type_id = tt.id
//                          LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id 
//                          LEFT JOIN category c on sc.category_id = c.id
//                          LEFT JOIN payerpayee pp on transaction.payerpayee_id = pp.id
//                  WHERE u.user_identifier = @user_identifier
//                 ORDER BY transaction.transaction_timestamp
//                  ";
//         }
        return await _transactionRepository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
            new AndSpec(new List<ITransactionSpecification>()));
    }

    private async Task<Guid> WritePayerPayeeIntoDb(PayerPayee payerPayee, string payerPayeeType)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var createPayerPayeeQuery =
                @"
                    WITH ins (payerPayeeId, userId, payerPayeeName, payerPayeeType, externalLinkType, externalId)
                             AS (VALUES (@payerPayeeId, @userId, @payerPayeeName, @payerPayeeType, @externalLinkId, @externalId)),
                         e AS (
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
                userId = TestUserId,
                payerPayeeName = payerPayee.PayerPayeeName,
                payerPayeeType,
                externalLinkId = string.IsNullOrEmpty(payerPayee.ExternalId) ? "Custom" : "Google",
                externalId = payerPayee.ExternalId
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
                    new {user_id = TestUserId, payerPayeeType});

            return Mapper.Map<List<TransactionService.Repositories.CockroachDb.Entities.PayerPayee>, List<PayerPayee>>(
                payersPayees.ToList());
        }
    }

    private async Task<Guid> WriteCategoryIntoDb(IDbConnection connection, Category category)
    {
        var createCategoryQuery =
            @"
                WITH e AS (
                    INSERT INTO category (name, user_id, transaction_type_id) 
                        VALUES (@category_name, @user_id, @transaction_type_id)
                        ON CONFLICT (name, user_id, transaction_type_id) DO NOTHING
                        RETURNING id
                )
                SELECT * FROM e
                UNION
                SELECT id FROM category c WHERE c.name = @category_name
                    AND c.user_id = @user_id
                    AND c.transaction_type_id = @transaction_type_id;               
            ";

        var categoryId = await connection.QuerySingleAsync<Guid>(createCategoryQuery,
            new
            {
                category_name = category.CategoryName, user_id = TestUserId,
                transaction_type_id = category.TransactionType == TransactionType.Expense
                    ? TransactionTypeIds.Expense
                    : TransactionTypeIds.Income
            });

        return categoryId;
    }

    private async Task<Guid> WriteSubcategoryIntoDb(IDbConnection connection, Guid categoryId, string subcategoryName)
    {
        var subcategoryQuery =
            @"
                WITH e AS (
                    INSERT INTO subcategory (name, category_id)
                    VALUES (@subcategoryName, @categoryId)
                    ON CONFLICT (name, category_id) DO UPDATE SET name = excluded.name
                    RETURNING id
                )
                SELECT * FROM e
                UNION
                SELECT id FROM subcategory sc WHERE sc.category_id = @categoryId AND sc.name = @subcategoryName;
            ";
        return await connection.QuerySingleAsync<Guid>(subcategoryQuery,
            new {subcategoryName, categoryId});
    }

    public async Task<Dictionary<Guid, List<Guid>>> WriteCategoriesIntoDb(List<Category> categories)
    {
        var categoryAndSubcategoryIdMap = new Dictionary<Guid, List<Guid>>();
        using (var connection = DapperContext.CreateConnection())
        {
            foreach (var category in categories)
            {
                var categoryId = await WriteCategoryIntoDb(connection, category);

                foreach (var subcategory in category.Subcategories)
                {
                    var subcategoryId = await WriteSubcategoryIntoDb(connection, categoryId, subcategory);

                    var subcategories = categoryAndSubcategoryIdMap.GetValueOrDefault(categoryId, new List<Guid>());
                    subcategories.Add(subcategoryId);
                    categoryAndSubcategoryIdMap[categoryId] = subcategories;
                }
            }
        }

        return categoryAndSubcategoryIdMap;
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

            return Mapper.Map<List<TransactionService.Repositories.CockroachDb.Entities.Category>, List<Category>>(
                categories.ToList());
        }
    }
}