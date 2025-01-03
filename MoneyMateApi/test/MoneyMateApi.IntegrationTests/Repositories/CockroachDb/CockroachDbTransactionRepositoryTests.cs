using System;
using System.Collections.Generic;
using System.Linq;
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
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();

        var transactionList = transactionListBuilder
            .WithTransactions(1, "8504f165-29f9-4873-97d5-84ad005dda60", payee1.ToString(), "payee_name_1", 24,
                TransactionType.Expense,
                "category1", "subcategory1", "note", [tagId1])
            .WithTransactions(1, "9504f165-29f9-4873-97d5-84ad005dda60", payee2.ToString(), "payee_name_2", 50,
                TransactionType.Expense,
                "category2", "subcategory2", null, [tagId1, tagId2])
            .BuildDomainModels();

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

                Assert.Single(transaction.TagIds);
                Assert.Equal(tagId1, transaction.TagIds.First());
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

                var expectedTags = new List<Guid> { tagId1, tagId2 };
                Assert.Equal(expectedTags.OrderBy(x => x), transaction.TagIds.OrderBy(x => x));
            });
    }

    [Fact]
    public async Task GivenTransactionDomainModel_WhenStoreTransactionInvoked_ThenTransactionStoredInDatabase()
    {
        // Arrange
        var repo = new CockroachDbTransactionRepository(_dapperContext, _stubMapper, new CurrentUserContext
        {
            UserId = _cockroachDbIntegrationTestHelper.TestUserIdentifier,
            ProfileId = _profileId
        });

        const decimal expectedAmount = 123M;
        const string expectedCategory = "Food";
        const string expectedSubcategory = "Dinner";
        var expectedTransactionTimestamp =
            new DateTimeOffset(new DateTime(2021, 4, 2), TimeSpan.Zero).ToString("yyyy-MM-ddThh:mm:ss.FFFK");
        const string expectedTransactionType = "expense";
        var expectedPayerPayeeId = Guid.NewGuid().ToString();
        const string expectedPayerPayeeName = "name1";
        const string expectedNote = "this is a note123";

        var tagId1 = Guid.Parse("59a5b67d-e01a-4d35-8028-cdd7ac5cb868");
        var tagId2 = Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee35");
        var inputTransaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            TransactionTimestamp = expectedTransactionTimestamp,
            TransactionType = expectedTransactionType,
            Amount = expectedAmount,
            Category = expectedCategory,
            Subcategory = expectedSubcategory,
            PayerPayeeId = expectedPayerPayeeId,
            PayerPayeeName = expectedPayerPayeeName,
            Note = expectedNote,
            TagIds = [tagId1, tagId2]
        };

        await _cockroachDbIntegrationTestHelper.CategoryOperations.WriteCategoriesIntoDb([
            new()
            {
                TransactionType = TransactionTypeExtensions.ConvertToTransactionType(expectedTransactionType),
                Subcategories = [expectedSubcategory],
                CategoryName = expectedCategory
            }
        ]);

        await _cockroachDbIntegrationTestHelper.PayerPayeeOperations.WritePayeesIntoDb([
            new PayerPayee
            {
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                ExternalId = "1234"
            }
        ]);

        await _cockroachDbIntegrationTestHelper.TagOperations.WriteTagsIntoDb([
            new Tag(tagId1, "tag1"), new Tag(tagId2, "tag2")
        ]);

        // Act
        await repo.StoreTransaction(inputTransaction);

        // Assert
        var transactionsInDb = await _cockroachDbIntegrationTestHelper.TransactionOperations.GetAllTransactions();

        Assert.Single(transactionsInDb);

        Assert.Equal(inputTransaction, transactionsInDb[0]);
    }
}