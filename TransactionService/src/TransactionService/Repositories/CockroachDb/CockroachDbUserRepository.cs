using System.Threading.Tasks;
using Dapper;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;

namespace TransactionService.Repositories.CockroachDb;

public class CockroachDbUserRepository : IUserRepository
{
    private readonly CurrentUserContext _userContext;
    private readonly DapperContext _dapperContext;

    public CockroachDbUserRepository(DapperContext dapperContext, CurrentUserContext userContext)
    {
        _dapperContext = dapperContext;
        _userContext = userContext;
    }

    public async Task<User> GetUser()
    {
        var query = @"SELECT id, user_identifier as userIdentifier FROM users WHERE user_identifier = @user_identifier";

        using (var connection = _dapperContext.CreateConnection())
        {
            var user = await connection.QuerySingleAsync<User>(query, new {user_identifier = _userContext.UserId});
            return user;
        }
    }
}