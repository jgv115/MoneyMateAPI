using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.PayerPayees;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Repositories.Exceptions;
using MoneyMateApi.Tests.Common;
using Xunit;

namespace MoneyMateApi.IntegrationTests.Repositories.CockroachDb;

[Collection("IntegrationTests")]
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

    #region CreatePayer

    [Fact]
    public async Task GivenPayerWithNoExternalId_WhenCreatePayerInvoked_ThenCorrectPayerPayeeEntityWrittenToDb()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var insertedPayer = new PayerPayee
        {
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name",
        };
        await repo.CreatePayerOrPayee(PayerPayeeType.Payer, insertedPayer);

        var payers = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payer");

        Assert.Collection(payers, payer => Assert.Equal(insertedPayer, payer));
    }

    [Fact]
    public async Task GivenPayerWithExternalId_WhenCreatePayerInvoked_ThenCorrectPayerPayeeEntityWrittenToDb()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var insertedPayee = new PayerPayee
        {
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name",
            ExternalId = "id123"
        };

        await repo.CreatePayerOrPayee(PayerPayeeType.Payee, insertedPayee);

        var payers = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payee");

        Assert.Collection(payers, payer => Assert.Equal(insertedPayee, payer));
    }


    [Fact]
    public async Task GivenPayerInsertedTwice_WhenCreatePayerInvoked_ThenExceptionthrown()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var insertedPayer = new PayerPayee
        {
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name",
            ExternalId = "id123"
        };
        await repo.CreatePayerOrPayee(PayerPayeeType.Payer, insertedPayer);
        await Assert.ThrowsAsync<RepositoryItemExistsException>(() =>
            repo.CreatePayerOrPayee(PayerPayeeType.Payer, insertedPayer));
    }

    #endregion

    #region PutPayerOrPayee

    [Fact]
    public async Task
        GivenExistingPayerWithExternalId_WhenPutPayerOrPayeeInvokedWithPayerPayeeWithNoExternalId_ThenPayerIsUpdatedCorrectly()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var insertedPayer = new PayerPayee
        {
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name",
            ExternalId = "id123"
        };
        await repo.CreatePayerOrPayee(PayerPayeeType.Payer, insertedPayer);

        var modifiedPayer = insertedPayer with
        {
            ExternalId = ""
        };
        await repo.PutPayerOrPayee(PayerPayeeType.Payer, modifiedPayer);

        var payers = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payer");

        Assert.Collection(payers, payer => Assert.Equal(modifiedPayer, payer));
    }

    [Fact]
    public async Task
        GivenExistingPayerWithExternalId_WhenPutPayerOrPayeeInvokedWithPayerPayeeWithDifferentExternalId_ThenPayerIsUpdatedCorrectly()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var insertedPayer = new PayerPayee
        {
            PayerPayeeId = Guid.NewGuid().ToString(),
            PayerPayeeName = "name",
            ExternalId = "id123"
        };
        await repo.CreatePayerOrPayee(PayerPayeeType.Payer, insertedPayer);

        var modifiedPayer = insertedPayer with
        {
            ExternalId = "id321"
        };
        await repo.PutPayerOrPayee(PayerPayeeType.Payer, modifiedPayer);

        var payers = await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.RetrieveAllPayersPayees("payer");

        Assert.Collection(payers, payer => Assert.Equal(modifiedPayer, payer));
    }

    #endregion

    #region GetSuggestedPayersOrPayees

    [Fact]
    public async Task
        GivenGeneralSuggestionSpecAndPayerType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedListOfPayersReturned()
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
            .BuildDomainModels();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees =
            await repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payer, new GeneralPayerPayeeSuggestionParameters());
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
        GivenSubcategorySuggestionSpecAndPayerType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedListOfPayersReturned()
    {
        var repo = new CockroachDbPayerPayeeRepository(_dapperContext, _stubMapper, new CurrentUserContext()
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payer1 = Guid.NewGuid(), payer2 = Guid.NewGuid(), payer3 = Guid.NewGuid(), payer4 = Guid.NewGuid();
        var transactionList = transactionListBuilder
            .WithTransactions(4, payer1.ToString(), "name", 24, TransactionType.Income, "category1", "subcategory1",
                null)
            .WithTransactions(8, payer2.ToString(), "name2", 24, TransactionType.Income, "category1", "subcategory1",
                null)
            .WithTransactions(6, payer3.ToString(), "name3", 24, TransactionType.Income, "category1", "subcategory1",
                null)
            .WithTransactions(10, payer4.ToString(), "name4", 24, TransactionType.Income, "category1", "subcategory1",
                null)
            .WithTransactions(15, payer1.ToString(), "name", 24, TransactionType.Income, "category1", "subcategory2",
                null)
            .BuildDomainModels();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees = await repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
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
        GivenRequestWithLimitAndPayerType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectNumberOfPayersReturned()
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
            .BuildDomainModels();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);


        var suggestedPayerPayees =
            await repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payer, new GeneralPayerPayeeSuggestionParameters(), 2);
        Assert.Equal(2, suggestedPayerPayees.Count());
    }

    [Fact]
    public async Task
        GivenGeneralSuggestionSpecAndPayeeType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedListOfPayeesReturned()
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
            .BuildDomainModels();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var suggestedPayerPayees =
            await repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payee, new GeneralPayerPayeeSuggestionParameters());

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
        GivenRequestWithLimitAndPayeeType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectNumberOfPayeesReturned()
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
            .BuildDomainModels();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);


        var suggestedPayerPayees =
            await repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payee, new GeneralPayerPayeeSuggestionParameters(),
                2);
        Assert.Equal(2, suggestedPayerPayees.Count());
    }

    #endregion
}