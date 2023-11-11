using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using TransactionService.Constants;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.CockroachDb.Profiles;
using Category = TransactionService.Domain.Models.Category;
using PayerPayee = TransactionService.Domain.Models.PayerPayee;
using Profile = TransactionService.Domain.Models.Profile;
using Transaction = TransactionService.Domain.Models.Transaction;
using TransactionType = TransactionService.Constants.TransactionType;

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
    public string TestUserIdentifier { get; } = "auth0|moneymatetest";
    private TransactionTypeIds TransactionTypeIds { get; set; }
    private IMapper Mapper { get; init; }
    private CockroachDbTransactionRepository TransactionRepository { get; init; }
    private CockroachDbCategoriesRepository CategoriesRepository { get; init; }
    private CockroachDbProfilesRepository ProfilesRepository { get; init; }

    public CockroachDbIntegrationTestHelper(Guid testUserId)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");

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

        TestUserId = testUserId;
        TransactionRepository = new CockroachDbTransactionRepository(DapperContext, Mapper, new CurrentUserContext
        {
            UserId = TestUserIdentifier,
            ProfileId = TestUserId
        });

        CategoriesRepository = new CockroachDbCategoriesRepository(DapperContext, Mapper, new CurrentUserContext
        {
            UserId = TestUserIdentifier,
            ProfileId = TestUserId
        });

        ProfilesRepository = new CockroachDbProfilesRepository(DapperContext, new CurrentUserContext
        {
            UserId = TestUserIdentifier,
            ProfileId = TestUserId
        });
    }

    public async Task SeedRequiredData()
    {
        using (var connection = DapperContext.CreateConnection())
        {
            // Create a test user
            await WriteUsersIntoDb(new List<User>
            {
                new()
                {
                    Id = TestUserId,
                    UserIdentifier = TestUserIdentifier
                }
            });

            // Create a profile for the test user
            await WriteProfilesIntoDbForUser(new List<Profile>
            {
                new()
                {
                    Id = TestUserId,
                    DisplayName = "Default Profile"
                }
            }, TestUserId);

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
            var query = @"TRUNCATE users, profile, category, subcategory, payerpayee CASCADE";

            await connection.ExecuteAsync(query);
        }
    }

    public async Task WriteUsersIntoDb(List<User> users)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var insertUsersQuery =
                @"INSERT INTO users (id, user_identifier) VALUES (@id, @userIdentifier)";

            await connection.ExecuteAsync(insertUsersQuery, users);
        }
    }

    public async Task WriteProfilesIntoDbForUser(List<Profile> profiles, Guid userId)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var insertProfilesQuery = @"INSERT INTO profile (id, display_name) VALUES (@id, @displayName)";
            await connection.ExecuteAsync(insertProfilesQuery, profiles);

            var insertUserProfileQuery = @"INSERT INTO userprofile (user_id, profile_id) VALUES (@userId, @profileId)";
            await connection.ExecuteAsync(insertUserProfileQuery,
                profiles.Select(profile => new
                {
                    userId,
                    profileId = profile.Id
                }));
        }
    }

    public Task<List<Profile>> RetrieveProfiles() => ProfilesRepository.GetProfiles();

    public async Task WriteTransactionsIntoDb(List<Transaction> transactions)
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var createTransactionQuery = @"
                INSERT INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerpayee_id, notes, profile_id)
                VALUES (@transactionId, @userId, @timestamp, @transactionTypeId, @amount, @subcategoryId, @payerpayeeId, @notes, @profileId)
            ";

            foreach (var transaction in transactions)
            {
                var parameters = new DynamicParameters();
                parameters.Add("transactionId", transaction.TransactionId);
                parameters.Add("userId", TestUserId);
                parameters.Add("profileId", TestUserId);
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
        return await TransactionRepository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
            new AndSpec(new List<ITransactionSpecification>()));
    }

    public async Task<Transaction> GetTransactionById(string transactionId)
    {
        return await TransactionRepository.GetTransactionById(transactionId);
    }

    private async Task<Guid> WritePayerPayeeIntoDb(PayerPayee payerPayee, string payerPayeeType)
    {
        using (var connection = DapperContext.CreateConnection())
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
                userId = TestUserId,
                profileId = TestUserId,
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
                    INSERT INTO category (name, user_id, transaction_type_id, profile_id) 
                        VALUES (@category_name, @user_id, @transaction_type_id, @profile_id)
                        ON CONFLICT (name, profile_id, transaction_type_id) DO NOTHING
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
                    : TransactionTypeIds.Income,
                profile_id = TestUserId
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
        return (await CategoriesRepository.GetAllCategories()).ToList();
    }

    public async Task<Category> RetrieveCategory(string categoryName)
    {
        return (await CategoriesRepository.GetAllCategories()).FirstOrDefault(category =>
            category.CategoryName == categoryName);
    }
}