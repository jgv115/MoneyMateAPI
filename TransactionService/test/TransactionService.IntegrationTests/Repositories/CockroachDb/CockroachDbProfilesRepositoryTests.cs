using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.Exceptions;
using Xunit;

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
    public async Task GivenUserAndProfileInDb_WhenGetProfileInvoked_ThenCorrectProfileReturned()
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

        var profile = await repository.GetProfile(expectedProfiles[0].Id);
        
        Assert.Equal(expectedProfiles[0], profile);
    }

    [Fact]
    public async Task GivenUserAndProfileNotInDb_WhenGetProfileInvoked_ThenRepositoryItemDoesNotExistExceptionThrown()
    {
        var testUser = new User
        {
            UserIdentifier = "test-user-123",
            Id = Guid.NewGuid()
        };

        var profiles = new List<Profile>
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
        });

        await _cockroachDbIntegrationTestHelper.WriteProfilesIntoDbForUser(profiles, testUser.Id);

        var repository = new CockroachDbProfilesRepository(_dapperContext, new CurrentUserContext
        {
            UserId = testUser.UserIdentifier
        });

        await Assert.ThrowsAsync<RepositoryItemDoesNotExist>(() => repository.GetProfile(Guid.NewGuid()));
    }
    
    [Fact]
    public async Task GivenUsersAndProfilesInDb_WhenGetUserProfilesInvoked_ThenCorrectProfileObjectsReturned()
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

        Assert.Collection(returnedProfiles.OrderBy(profile => profile.DisplayName), profile =>
            {
                Assert.Equal(expectedProfiles[0].DisplayName, profile.DisplayName);
                Assert.Equal(expectedProfiles[0].Id, profile.Id);
            },
            profile =>
            {
                Assert.Equal(expectedProfiles[1].DisplayName, profile.DisplayName);
                Assert.Equal(expectedProfiles[1].Id, profile.Id);
            });
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

    [Fact]
    public async Task GivenNewDisplayName_WhenCreateProfileInvoked_ThenDbContainsCorrectRecordsForNewProfileAndProfileIdReturned()
    {
        var testUser = new User
        {
            UserIdentifier = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            Id = Guid.NewGuid()
        };
        var otherUser = new User
        {
            UserIdentifier = "test-user-456",
            Id = Guid.NewGuid()
        };

        await _cockroachDbIntegrationTestHelper.WriteUsersIntoDb(new List<User>
        {
            testUser,
            otherUser
        });

        var repository = new CockroachDbProfilesRepository(_dapperContext, new CurrentUserContext
        {
            UserId = testUser.UserIdentifier
        });

        const string expectedNewProfileName = "new profile name";
        var returnedProfileId = await repository.CreateProfile(expectedNewProfileName);

        var returnedProfiles = await _cockroachDbIntegrationTestHelper.RetrieveProfiles();

        Assert.Collection(returnedProfiles, profile =>
        {
            Assert.Equal(expectedNewProfileName, profile.DisplayName);
            Assert.Equal(returnedProfileId, profile.Id);
        });
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