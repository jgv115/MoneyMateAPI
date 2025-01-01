using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Repositories;
using MoneyMateApi.Repositories.CockroachDb;

namespace MoneyMateApi.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestTagOperations
{
    private readonly DapperContext _dapperContext;
    private readonly ITagRepository _tagRepository;
    private readonly Guid _profileId;

    public CockroachDbIntegrationTestTagOperations(DapperContext dapperContext, ITagRepository tagRepository,
        Guid profileId)
    {
        _dapperContext = dapperContext;
        _tagRepository = tagRepository;
        _profileId = profileId;
    }

    public async Task<List<Tag>> WriteTagsIntoDb(List<string> tagNames)
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var insertTagsQuery = @"INSERT INTO tag (name, profile_id) VALUES (@name, @profile_id) RETURNING id";

            var insertedTags = new List<Tag>();
            foreach (var tagName in tagNames)
            {
                var insertedId = await connection.QuerySingleAsync<Guid>(insertTagsQuery,
                    new { name = tagName, profile_id = _profileId });

                insertedTags.Add(new Tag(insertedId.ToString(), tagName));
            }


            return insertedTags;
        }
    }

    public async Task<List<MoneyMateApi.Repositories.CockroachDb.Entities.Tag>> GetTagsForProfile()
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var retrieveTagsQuery = @"SELECT id, name, profile_id as profileid, created_at as createdAt  
                                        FROM tag WHERE profile_id = @profile_id";
            var tagEntities = await connection.QueryAsync<MoneyMateApi.Repositories.CockroachDb.Entities.Tag>(
                retrieveTagsQuery,
                new { profile_id = _profileId });
            return tagEntities.ToList();
        }
    }
    
    public async Task<List<MoneyMateApi.Repositories.CockroachDb.Entities.Tag>> GetAllTagsFromDb()
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            var retrieveTagsQuery = @"SELECT id, name, profile_id as profileid, created_at as createdAt  
                                        FROM tag";
            var tagEntities = await connection.QueryAsync<MoneyMateApi.Repositories.CockroachDb.Entities.Tag>(
                retrieveTagsQuery,
                new { profile_id = _profileId });
            return tagEntities.ToList();
        }
    }
}