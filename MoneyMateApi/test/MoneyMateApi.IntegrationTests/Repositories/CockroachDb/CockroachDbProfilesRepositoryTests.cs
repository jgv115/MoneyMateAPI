using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Profiles;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Entities;
using MoneyMateApi.Repositories.Exceptions;
using Xunit;

namespace MoneyMateApi.IntegrationTests.Repositories.CockroachDb;

[Collection("IntegrationTests")]
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

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            testUser,
            otherUser
        });

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(expectedProfiles,
            testUser.Id);
        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(otherProfiles,
            otherUser.Id);

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

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            testUser,
        });

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(profiles, testUser.Id);

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

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            testUser,
            otherUser
        });

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(expectedProfiles,
            testUser.Id);
        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteProfilesIntoDbForUser(otherProfiles,
            otherUser.Id);


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
    public async Task
        GivenNewDisplayName_WhenCreateProfileInvoked_ThenDbContainsCorrectRecordsForNewProfileAndProfileIdReturned()
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

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
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

        var returnedProfiles = await _cockroachDbIntegrationTestHelper.UserProfileOperations.RetrieveProfiles();

        Assert.Collection(returnedProfiles, profile =>
        {
            Assert.Equal(expectedNewProfileName, profile.DisplayName);
            Assert.Equal(returnedProfileId, profile.Id);
        });
    }

    [Fact]
    public async Task
        GivenNewDisplayNameAndProfileId_WhenCreateProfileInvoked_ThenDbContainsCorrectRecordsForNewProfileAndProfileIdReturned()
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

        await _cockroachDbIntegrationTestHelper.UserProfileOperations.WriteUsersIntoDb(new List<User>
        {
            testUser,
            otherUser
        });

        var repository = new CockroachDbProfilesRepository(_dapperContext, new CurrentUserContext
        {
            UserId = testUser.UserIdentifier
        });

        const string expectedNewProfileName = "new profile name";
        var expectedProfileId = Guid.NewGuid();
        var returnedProfileId = await repository.CreateProfile(expectedNewProfileName, expectedProfileId);

        var returnedProfiles = await _cockroachDbIntegrationTestHelper.UserProfileOperations.RetrieveProfiles();

        Assert.Equal(expectedProfileId, returnedProfileId);
        Assert.Collection(returnedProfiles, profile =>
        {
            Assert.Equal(expectedNewProfileName, profile.DisplayName);
            Assert.Equal(returnedProfileId, profile.Id);
        });
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