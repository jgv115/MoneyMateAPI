using System;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using Xunit;

namespace TransactionService.IntegrationTests.Repositories.CockroachDb;

public class CockroachDbCategoriesRepositoryTests : IClassFixture<CockroachDbIntegrationTestHelper>
{
    private readonly DapperContext _dapperContext;
    private readonly IMapper _stubMapper;
    private readonly CurrentUserContext _stubUserContext;

    public CockroachDbCategoriesRepositoryTests(CockroachDbIntegrationTestHelper cockroachDbIntegrationTestHelper)
    {
        _dapperContext = cockroachDbIntegrationTestHelper.DapperContext;
        _stubMapper =
            new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(CockroachDbCategoriesRepository)))
                .CreateMapper();
        _stubUserContext = new CurrentUserContext
        {
            UserId = "user-id-123"
        };
    }

    // [Fact]
    // public async Task Test()
    // {
    //     var repository = new CockroachDbCategoriesRepository(_dapperContext, _stubMapper, _stubUserContext);
    //
    //     await repository.GetCategory();
    // }
}