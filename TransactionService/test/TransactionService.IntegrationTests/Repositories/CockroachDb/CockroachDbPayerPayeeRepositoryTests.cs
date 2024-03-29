using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.PayerPayees;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb;
using TransactionService.Tests.Common;
using Xunit;

namespace TransactionService.IntegrationTests.Repositories.CockroachDb;

public class CockroachDbPayerPayeeRepositoryTests : IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _profileId;
    private readonly IMapper _stubMapper;

    public CockroachDbPayerPayeeRepositoryTests()
    {
        _profileId = Guid.NewGuid();
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(_profileId);
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
        _stubMapper =
            new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(CockroachDbPayerPayeeRepository)))
                .CreateMapper();
    }

    public async Task InitializeAsync()
    {
        await _cockroachDbIntegrationTestHelper.SeedRequiredData();
    }

    public async Task DisposeAsync()
    {
        await _cockroachDbIntegrationTestHelper.ClearDbData();
    }

    [Fact]
    public async Task
        GivenGeneralSuggestionSpec_WhenGetSuggestedPayersInvoked_ThenCorrectSuggestedListOfPayersReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payer1 = Guid.NewGuid(), payer2 = Guid.NewGuid(), payer3 = Guid.NewGuid(), payer4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(10, payer1.ToString(), "name", 24,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payer2.ToString(), "name2", 50,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payer3.ToString(), "name3", 50,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(2, payer4.ToString(), "name4", 50,
                TransactionType.Income)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees =
            await repo.GetSuggestedPayers(new GeneralPayerPayeeSuggestionParameters());
        Assert.Equal(new List<PayerPayee>()
        {
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name"
            },
            new()
            {
                PayerPayeeId = payer2.ToString(),
                PayerPayeeName = "name2"
            },
            new()
            {
                PayerPayeeId = payer3.ToString(),
                PayerPayeeName = "name3"
            },
            new()
            {
                PayerPayeeId = payer4.ToString(),
                PayerPayeeName = "name4"
            }
        }, suggestedPayerPayees);
    }

    [Fact]
    public async Task
        GivenSubcategorySuggestionSpec_WhenGetSuggestedPayersInvoked_ThenCorrectSuggestedListOfPayersReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payer1 = Guid.NewGuid(), payer2 = Guid.NewGuid(), payer3 = Guid.NewGuid(), payer4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithTransactions(4, payer1.ToString(), "name", 24, TransactionType.Income, "category1", "subcategory1")
            .WithTransactions(8, payer2.ToString(), "name2", 24, TransactionType.Income, "category1", "subcategory1")
            .WithTransactions(6, payer3.ToString(), "name3", 24, TransactionType.Income, "category1", "subcategory1")
            .WithTransactions(10, payer4.ToString(), "name4", 24, TransactionType.Income, "category1", "subcategory1")
            .WithTransactions(15, payer1.ToString(), "name", 24, TransactionType.Income, "category1", "subcategory2")
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees = await repo.GetSuggestedPayers(
            new SubcategoryPayerPayeeSuggestionParameters("category1", "subcategory1")
        );
        Assert.Equal(new List<PayerPayee>()
        {
            new()
            {
                PayerPayeeId = payer4.ToString(),
                PayerPayeeName = "name4"
            },
            new()
            {
                PayerPayeeId = payer2.ToString(),
                PayerPayeeName = "name2"
            },
            new()
            {
                PayerPayeeId = payer3.ToString(),
                PayerPayeeName = "name3"
            },
            new()
            {
                PayerPayeeId = payer1.ToString(),
                PayerPayeeName = "name"
            }
        }, suggestedPayerPayees);
    }

    [Fact]
    public async Task
        GivenRequestWithLimit_WhenGetSuggestedPayersInvoked_ThenCorrectNumberOfPayersReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payer1 = Guid.NewGuid(), payer2 = Guid.NewGuid(), payer3 = Guid.NewGuid(), payer4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(10, payer1.ToString(), "name", 24,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payer2.ToString(), "name2", 50,
                TransactionType.Income)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payer3.ToString(), "name3", 50,
                TransactionType.Income)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);


        var suggestedPayerPayees =
            await repo.GetSuggestedPayers(new GeneralPayerPayeeSuggestionParameters(), 2);
        Assert.Equal(2, suggestedPayerPayees.Count());
    }

    [Fact]
    public async Task
        GivenGeneralSuggestionSpec_WhenGetSuggestedPayeesInvoked_ThenCorrectSuggestedListOfPayeesReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payee1 = Guid.NewGuid(), payee2 = Guid.NewGuid(), payee3 = Guid.NewGuid(), payee4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(10, payee1.ToString(), "name", 24)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payee2.ToString(), "name2", 50)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payee3.ToString(), "name3", 50)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(2, payee4.ToString(), "name4", 50)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees =
            await repo.GetSuggestedPayees(new GeneralPayerPayeeSuggestionParameters());

        Assert.Equal(new List<PayerPayee>()
        {
            new()
            {
                PayerPayeeId = payee1.ToString(),
                PayerPayeeName = "name"
            },
            new()
            {
                PayerPayeeId = payee2.ToString(),
                PayerPayeeName = "name2"
            },
            new()
            {
                PayerPayeeId = payee3.ToString(),
                PayerPayeeName = "name3"
            },
            new()
            {
                PayerPayeeId = payee4.ToString(),
                PayerPayeeName = "name4"
            }
        }, suggestedPayerPayees);
    }

    [Fact]
    public async Task
        GivenRequestWithLimit_WhenGetSuggestedPayeesInvoked_ThenCorrectNumberOfPayeesReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payee1 = Guid.NewGuid(), payee2 = Guid.NewGuid(), payee3 = Guid.NewGuid(), payee4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(10, payee1.ToString(), "name", 24)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(6, payee2.ToString(), "name2", 50)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(5, payee3.ToString(), "name3", 50)
            .WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(2, payee4.ToString(), "name4", 50)
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);


        var suggestedPayerPayees =
            await repo.GetSuggestedPayees(new GeneralPayerPayeeSuggestionParameters(),
                2);
        Assert.Equal(2, suggestedPayerPayees.Count());
    }
}