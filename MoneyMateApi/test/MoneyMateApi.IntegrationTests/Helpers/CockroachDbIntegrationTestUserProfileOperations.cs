using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Repositories;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestUserProfileOperations
{
    private readonly DapperContext _dapperContext;
    private readonly IProfilesRepository _profilesRepository;

    public CockroachDbIntegrationTestUserProfileOperations(DapperContext dapperContext,
        IProfilesRepository profilesRepository)
    {
        _profilesRepository = profilesRepository;
        _dapperContext = dapperContext;
    }

    public async Task WriteUsersIntoDb(List<User> users)
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var insertUsersQuery =
                @"INSERT INTO users (id, user_identifier) VALUES (@id, @userIdentifier)";

            await connection.ExecuteAsync(insertUsersQuery, users);
        }
    }
    
    public async Task WriteProfilesIntoDbForUser(List<Profile> profiles, Guid userId)
    {
        using (var connection = _dapperContext.CreateConnection())
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

    public Task<IEnumerable<Profile>> RetrieveProfiles() => _profilesRepository.GetProfiles();
}