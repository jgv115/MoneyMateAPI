using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.IntegrationTests.Helpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb;
using MoneyMateApi.Tests.Common;
using Xunit;

namespace MoneyMateApi.IntegrationTests.Repositories.CockroachDb;

[Collection("IntegrationTests")]
public class CockroachDbTransactionRepositoryTests : IAsyncLifetime
{
    private readonly DapperContext _dapperContext;
    private readonly CockroachDbIntegrationTestHelper _cockroachDbIntegrationTestHelper;
    private readonly Guid _profileId;
    private readonly IMapper _stubMapper;

    public CockroachDbTransactionRepositoryTests()
    {
        _profileId = Guid.NewGuid();
        _cockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(_profileId);
        _dapperContext = _cockroachDbIntegrationTestHelper.DapperContext;
        _stubMapper =
            new MapperConfiguration(cfg =>
                    cfg.AddMaps(typeof(CockroachDbTransactionRepository)))
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
    public async Task GivenTransactionInDatabase_WhenGetTransactionsInvoked_ThenCorrectListOfTransactionsReturned()
    {
        var repo = new CockroachDbTransactionRepository(_dapperContext, _stubMapper, new CurrentUserContext
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        var transactionListBuilder = new TransactionListBuilder();

        Guid payee1 = Guid.NewGuid(), payee2 = Guid.NewGuid(), payee3 = Guid.NewGuid(), payee4 = Guid.NewGuid();
        var tag1 = new Tag(Guid.NewGuid(), "tag1");
        var tag2 = new Tag(Guid.NewGuid(), "tag2");

        var transactionList = transactionListBuilder
            .WithTransactions(1, payee1.ToString(), "payee_name_1", 24, TransactionType.Expense,
                "category1", "subcategory1", "note", [tag1])
            .WithTransactions(1, payee2.ToString(), "payee_name_2", 50, TransactionType.Expense,
                "category2", "subcategory2", null, [tag1, tag2])
            .Build();

        await _cockroachDbIntegrationTestHelper.TransactionOperations.WriteTransactionsIntoDb(transactionList);

        var transactionsInDb = await repo.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
            new AndSpec(new List<ITransactionSpecification>()));

        Assert.Collection(transactionsInDb, transaction =>
            {
                Assert.True(Guid.TryParse(transaction.TransactionId, out _));

                var timeDifference = DateTime.UtcNow - DateTime.Parse(transaction.TransactionTimestamp);
                Assert.True(timeDifference.TotalSeconds < 5);

                Assert.Equal(TransactionType.Expense.ToProperString(), transaction.TransactionType);
                Assert.Equal(24, transaction.Amount);
                Assert.Equal("category1", transaction.Category);
                Assert.Equal("subcategory1", transaction.Subcategory);
                Assert.Equal(payee1.ToString(), transaction.PayerPayeeId);
                Assert.Equal("payee_name_1", transaction.PayerPayeeName);
                Assert.Equal("note", transaction.Note);

                Assert.Single(transaction.Tags);
                Assert.Equal(tag1, transaction.Tags[0]);
            },
            transaction =>
            {
                Assert.True(Guid.TryParse(transaction.TransactionId, out _));

                var timeDifference = DateTime.UtcNow - DateTime.Parse(transaction.TransactionTimestamp);
                Assert.True(timeDifference.TotalSeconds < 5);

                Assert.Equal(TransactionType.Expense.ToProperString(), transaction.TransactionType);
                Assert.Equal(50, transaction.Amount);
                Assert.Equal("category2", transaction.Category);
                Assert.Equal("subcategory2", transaction.Subcategory);
                Assert.Equal(payee2.ToString(), transaction.PayerPayeeId);
                Assert.Equal("payee_name_2", transaction.PayerPayeeName);
                Assert.Equal("", transaction.Note);

                transaction.Tags.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
                Assert.Equal(2, transaction.Tags.Count);
                Assert.Equal(tag1, transaction.Tags[0]);
                Assert.Equal(tag2, transaction.Tags[1]);
            });
    }
}