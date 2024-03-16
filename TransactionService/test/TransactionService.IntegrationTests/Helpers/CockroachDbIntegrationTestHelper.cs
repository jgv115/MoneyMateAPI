using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Microsoft.Extensions.Configuration;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.CockroachDb.Profiles;
using Profile = TransactionService.Domain.Models.Profile;

namespace TransactionService.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestHelper
{
    public DapperContext DapperContext { get; init; }

    private Guid TestUserId { get; set; }

    // TODO: need to make this a bit easier to understand
    public string TestUserIdentifier { get; }
    private IMapper Mapper { get; init; }
    public CockroachDbIntegrationTestTransactionOperations TransactionOperations { get; init; }
    public CockroachDbIntegrationTestCategoryOperations CategoryOperations { get; init; }
    public CockroachDbIntegrationTestPayerPayeeOperations PayerPayeeOperations { get; init; }
    public CockroachDbIntegrationTestUserProfileOperations UserProfileOperations { get; init; }


    public CockroachDbIntegrationTestHelper(Guid testUserId) : this(testUserId, "auth0|moneymatetest")
    {
    }

    public CockroachDbIntegrationTestHelper(Guid testUserId, string testUserIdentifier)
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

        TestUserIdentifier = testUserIdentifier;
        TestUserId = testUserId;

        var transactionTypeOperations = new CockroachDbIntegrationTestTransactionTypeOperations(DapperContext);

        CategoryOperations = new CockroachDbIntegrationTestCategoryOperations(DapperContext,
            new CockroachDbCategoriesRepository(
                DapperContext, Mapper,
                new CurrentUserContext
                {
                    UserId = TestUserIdentifier,
                    ProfileId = TestUserId
                }),
            TestUserId,
            transactionTypeOperations
        );

        PayerPayeeOperations = new CockroachDbIntegrationTestPayerPayeeOperations(DapperContext, TestUserId);

        UserProfileOperations = new CockroachDbIntegrationTestUserProfileOperations(DapperContext,
            new CockroachDbProfilesRepository(DapperContext, new CurrentUserContext
            {
                UserId = TestUserIdentifier,
                ProfileId = TestUserId
            }));

        TransactionOperations = new CockroachDbIntegrationTestTransactionOperations(
            DapperContext,
            new CockroachDbTransactionRepository(DapperContext, Mapper, new CurrentUserContext
            {
                UserId = TestUserIdentifier,
                ProfileId = TestUserId
            }), TestUserId,
            transactionTypeOperations,
            CategoryOperations,
            PayerPayeeOperations
        );
    }

    public async Task SeedRequiredData()
    {
        // Create a test user
        await UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            new()
            {
                Id = TestUserId,
                UserIdentifier = TestUserIdentifier
            }
        });

        // Create a profile for the test user
        await UserProfileOperations.WriteProfilesIntoDbForUser(new List<Profile>
        {
            new()
            {
                Id = TestUserId,
                DisplayName = "Default Profile"
            }
        }, TestUserId);
    }

    public async Task ClearDbData()
    {
        using (var connection = DapperContext.CreateConnection())
        {
            var query = @"TRUNCATE users, profile, category, subcategory, payerpayee CASCADE";

            await connection.ExecuteAsync(query);
        }
    }
}