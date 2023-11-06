using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.CockroachDb;

public class CockroachDbProfilesRepository : IProfilesRepository
{
    private readonly DapperContext _context;
    private readonly CurrentUserContext _userContext;

    public CockroachDbProfilesRepository(DapperContext context, CurrentUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<List<Profile>> GetProfiles()
    {
        var query =
            @"SELECT p.id, p.display_name as displayName FROM userprofile up
                    LEFT JOIN users u on u.id = up.user_id
                    LEFT JOIN profile p on p.id = up.profile_id
                    WHERE u.user_identifier = @user_identifier";

        using (var connection = _context.CreateConnection())
        {
            // TODO: we are using the domain model directly here - entity model is not needed
            var userProfiles = (await connection.QueryAsync<Profile>(query, new
            {
                user_identifier = _userContext.UserId
            })).Distinct().ToList();

            if (!userProfiles.Any())
                throw new RepositoryItemDoesNotExist(
                    $"Could not find any profiles for userIdentifier: {_userContext.UserId}");

            return userProfiles;
        }
    }

    public async Task CreateProfile(string displayName)
    {
        using (var connection = _context.CreateConnection())
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                var createProfileQuery = @"INSERT INTO profile (display_name) VALUES (@displayName) RETURNING id";
                var profileId = await connection.QuerySingleAsync<Guid>(createProfileQuery, new {displayName});

                var createUserProfilesQuery = @"INSERT INTO userprofile (user_id, profile_id) 
                                                    SELECT u.id,  @profileId FROM users u where u.user_identifier = @user_identifier;";

                await connection.ExecuteAsync(createUserProfilesQuery,
                    new {profileId, user_identifier = _userContext.UserId});

                transaction.Commit();
            }
        }
    }
}