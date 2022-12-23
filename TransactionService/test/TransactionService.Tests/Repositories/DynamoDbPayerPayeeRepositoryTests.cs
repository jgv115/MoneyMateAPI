using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Repositories;

public class DynamoDbPayerPayeeRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock = new();
    private const string UserId = "test-user-123";
    private const string TableName = "table-name123";

    private readonly DynamoDbRepositoryConfig _stubConfig = new()
    {
        TableName = TableName
    };

    private readonly CurrentUserContext _userContext = new()
    {
        UserId = UserId
    };

    private static List<PayerPayee> RemoveSortKeyMetaData(List<PayerPayee> payerPayees) =>
        payerPayees.Select(payerPayee => new PayerPayee()
        {
            UserId = payerPayee.UserId,
            ExternalId = payerPayee.ExternalId,
            PayerPayeeId = payerPayee.PayerPayeeId.Split("#")[1],
            PayerPayeeName = payerPayee.PayerPayeeName
        }).ToList();


    public class GetPayeesPaginatedTestData : IEnumerable<object[]>
    {
        private readonly List<PayerPayee> _payees = new()
        {
            new()
            {
                UserId = $"{UserId}#PayersPayees",
                ExternalId = "123",
                PayerPayeeId = "payee#id1",
                PayerPayeeName = "name1"
            },
            new()
            {
                UserId = $"{UserId}#PayersPayees",
                ExternalId = "234",
                PayerPayeeId = "payee#id2",
                PayerPayeeName = "name2"
            },
            new()
            {
                UserId = $"{UserId}#PayersPayees",
                ExternalId = "345",
                PayerPayeeId = "payee#id3",
                PayerPayeeName = "name3"
            },
            new()
            {
                UserId = $"{UserId}#PayersPayees",
                ExternalId = "456",
                PayerPayeeId = "payee#id4",
                PayerPayeeName = "name4"
            }
        };

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new List<PayerPayee>(), 0, 2, new List<PayerPayee>()
            };
            yield return new object[]
            {
                _payees, 0, 3, RemoveSortKeyMetaData(new List<PayerPayee>
                {
                    _payees[0],
                    _payees[1],
                    _payees[2]
                })
            };
            yield return new object[]
            {
                _payees, 3, 2, RemoveSortKeyMetaData(new List<PayerPayee>
                {
                    _payees[3],
                })
            };
            yield return new object[] {_payees, 4, 2, new List<PayerPayee>()};
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Theory]
    [ClassData(typeof(GetPayeesPaginatedTestData))]
    public async Task GivenPaginationSpec_WhenGetPayeesInvoked_ThenCorrectPayeesReturned(List<PayerPayee> payees,
        int offset, int limit, List<PayerPayee> expectedReturnedPayees)
    {
        var repository = new DynamoDbPayerPayeeRepository(_stubConfig, _dynamoDbContextMock.Object);

        var mockAsyncSearch = new Mock<AsyncSearch<PayerPayee>>();
        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => payees);

        _dynamoDbContextMock.Setup(context => context.QueryAsync<PayerPayee>($"{UserId}#PayersPayees",
            QueryOperator.BeginsWith, new[] {"payee#"},
            It.Is<DynamoDBOperationConfig>(
                config => config.OverrideTableName == TableName))).Returns(mockAsyncSearch.Object);

        var returnedPayees = await repository.GetPayees(
            UserId, new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });

        Assert.Equal(expectedReturnedPayees, returnedPayees);
    }
}