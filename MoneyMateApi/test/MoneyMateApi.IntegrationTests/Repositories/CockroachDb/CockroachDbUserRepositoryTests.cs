using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using Xunit;

namespace MoneyMateApi.IntegrationTests.Repositories.CockroachDb;

[Collection("IntegrationTests")]
public class CockroachDbUserRepositoryTests: IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;

    public CockroachDbUserRepositoryTests()
    {
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(Guid.NewGuid());
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
    }

    [Fact]
    public async Task GivenUserIdentifier_WhenGetUserInvoked_ThenCorrectUserReturned()
    {
        var testUser = new User
        {
            UserIdentifier = "test-user-123",
            Id = Guid.NewGuid()
        };

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>()
        {
            testUser
        });

        var repository = new CockroachDbUserRepository(_dapperContext, new CurrentUserContext
        {
            UserId = testUser.UserIdentifier
        });

        var returnedUser = await repository.GetUser();

        Assert.Equal(testUser, returnedUser);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }
}