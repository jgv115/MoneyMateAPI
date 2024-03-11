using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using Xunit;

namespace TransactionService.IntegrationTests.Repositories.CockroachDb;

public class CockroachDbCategoriesRepositoryTests : IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly IMapper _stubMapper;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _profileId;

    public CockroachDbCategoriesRepositoryTests()
    {
        _profileId = Guid.NewGuid();
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(_profileId);
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
        _stubMapper =
            new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(CockroachDbCategoriesRepository)))
                .CreateMapper();
    }

    [Fact]
    public async Task
        GivenExistingProfileWithSameSubcategoryName_WhenAddSubcategoryInvoked_ThenSubcategoryIsAddedToCorrectProfile()
    {
        await _cockroachDbIntegrationTestHelper.CategoryOperations.WriteCategoriesIntoDb(new List<Category>
        {
            new()
            {
                TransactionType = TransactionType.Expense,
                CategoryName = "Travel"
            }
        });

        // Insert same category with another profile
        var integrationTestHelper2 = new CockroachDbIntegrationTestHelper(Guid.NewGuid(), "other_user_identifier");
        await integrationTestHelper2.SeedRequiredData();
        await integrationTestHelper2.CategoryOperations.WriteCategoriesIntoDb(new List<Category>
        {
            new()
            {
                TransactionType = TransactionType.Expense,
                CategoryName = "Travel",
                Subcategories = new List<string>() {"Accommodation"}
            }
        });

        var repository = new CockroachDbCategoriesRepository(_dapperContext, _stubMapper, new CurrentUserContext
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        await repository.AddSubcategory("Travel", "Accommodation");

        var category = await _cockroachDbIntegrationTestHelper.CategoryOperations.RetrieveCategory("Travel");

        Assert.Equal(new Category
        {
            TransactionType = TransactionType.Expense,
            CategoryName = "Travel",
            Subcategories = new List<string> {"Accommodation"}
        }, category);
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }
}