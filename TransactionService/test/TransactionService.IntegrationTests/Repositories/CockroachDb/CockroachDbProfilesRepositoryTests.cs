using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.Exceptions;
using Xunit;
using Profile = TransactionService.Repositories.CockroachDb.Entities.Profile;

namespace TransactionService.IntegrationTests.Repositories.CockroachDb;

public class CockroachDbProfilesRepositoryTests : IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;

    public CockroachDbProfilesRepositoryTests()
    {
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(Guid.NewGuid());
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
    }

    [Fact]
    public async Task GivenUsersAndProfilesInDb_WhenGetUserProfilesInvoked_ThenCorrectUserProfilesObjectReturned()
    {
        var testUser = new User
        {
            UserIdentifier = "test-user-123",
            Id = Guid.NewGuid()
        };
        var otherUser = new User
        {
            UserIdentifier = "test-user-456",
            Id = Guid.NewGuid()
        };

        var expectedProfiles = new List<Profile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Default Profile"
            },
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Partner Profile 1"
            }
        };

        var otherProfiles = new List<Profile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Default Profile"
            },
            new()
            {
                Id = Guid.NewGuid(),
                DisplayName = "Partner Profile 1"
            }
        };

        await _cockroachDbIntegrationTestHelper.WriteUsersIntoDb(new List<User>
        {
            testUser,
            otherUser
        });


        await _cockroachDbIntegrationTestHelper.WriteProfilesIntoDbForUser(expectedProfiles, testUser.Id);
        await _cockroachDbIntegrationTestHelper.WriteProfilesIntoDbForUser(otherProfiles, otherUser.Id);


        var repository = new CockroachDbProfilesRepository(_dapperContext, new CurrentUserContext
        {
            UserId = testUser.UserIdentifier
        });

        var returnedProfiles = await repository.GetProfiles();

        Assert.Equal(expectedProfiles, returnedProfiles);
    }


    [Fact]
    public async Task GivenUserNotFoundInDb_WhenGetUserProfilesInvoked_ThenRepositoryItemDoesNotExistExceptionThrown()
    {
        var repository = new CockroachDbProfilesRepository(_dapperContext, new CurrentUserContext
        {
            UserId = "random user identifier"
        });

        await Assert.ThrowsAsync<RepositoryItemDoesNotExist>(() => repository.GetProfiles());
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}