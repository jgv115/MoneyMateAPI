using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MoneyMateApi.Domain.Tags;
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

    public async Task<List<Tag>> WriteTagsIntoDb(List<Tag> tags)
    {
        using (var connection = _dapperContext.CreateConnection())
        {
            const string insertTagsQuery = """
                                           WITH inserted AS (
                                               INSERT INTO tag (id, name, profile_id)
                                               VALUES (@id, @name, @profile_id)
                                               ON CONFLICT DO NOTHING
                                               RETURNING id
                                           )
                                           SELECT * FROM inserted
                                           UNION
                                           SELECT id FROM tag WHERE name = @name AND profile_id = @profile_id
                                           """;

            var insertedTags = new List<Tag>();
            foreach (var tag in tags)
            {
                var insertedId = await connection.QuerySingleAsync<Guid>(insertTagsQuery,
                    new { id = tag.Id, name = tag.Name, profile_id = _profileId });

                insertedTags.Add(tag);
            }

            return insertedTags;
        }
    }

    public async Task AssociateTagWithTransaction(string transactionId, Guid tagId)
    {
        using var connection = _dapperContext.CreateConnection();
        const string insertTransactionTagQuery =
            @"INSERT INTO transactiontags (transaction_id, tag_id) VALUES (@transaction_id, @tag_id)";

        await connection.ExecuteAsync(insertTransactionTagQuery,
            new { transaction_id = transactionId, tag_id = tagId });
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