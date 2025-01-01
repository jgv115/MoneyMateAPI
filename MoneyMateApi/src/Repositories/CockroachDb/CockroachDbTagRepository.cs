using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.Exceptions;
using Npgsql;

namespace MoneyMateApi.Repositories.CockroachDb;

public class CockroachDbTagRepository : ITagRepository
{
    private readonly DapperContext _context;
    private readonly IMapper _mapper;
    private readonly CurrentUserContext _userContext;

    public CockroachDbTagRepository(DapperContext context, IMapper mapper, CurrentUserContext userContext)
    {
        _userContext = userContext;
        _mapper = mapper;
        _context = context;
    }

    public async Task<List<Tag>> GetTags()
    {
        var query =
            @"
            SELECT id, name, profile_id as profileid, created_at as createdAt  FROM tag
            WHERE profile_id = @ProfileId   
            ";

        using (var connection = _context.CreateConnection())
        {
            var tags = await connection.QueryAsync<Entities.Tag>(query, new { _userContext.ProfileId });
            return _mapper.Map<List<Tag>>(tags);
        }
    }

    public async Task<Tag> GetTag(string id)
    {
        var query =
            @"
            SELECT id, name, profile_id as profileid, created_at as createdAt  FROM tag
            WHERE profile_id = @ProfileId and id = @Id
            ";

        using (var connection = _context.CreateConnection())
        {
            var tag = await connection.QuerySingleAsync<Entities.Tag>(query, new { id, _userContext.ProfileId });
            return _mapper.Map<Tag>(tag);
        }
    }

    public async Task<Tag> CreateTag(string name)
    {
        var query =
            @"INSERT INTO tag (name, profile_id) VALUES(@Name, @ProfileId) RETURNING id";

        using (var connection = _context.CreateConnection())
        {
            try
            {
                var storedId =
                    await connection.QuerySingleAsync<Guid>(query, new { Name = name, _userContext.ProfileId });
                return new Tag(storedId.ToString(), name);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                {
                    throw new RepositoryItemExistsException("Tag already exists");
                }

                throw;
            }
        }
    }
}