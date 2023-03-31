using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;
using TransactionService.Repositories.DynamoDb.Models;
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

    private readonly CurrentUserContext _currentUserContext = new CurrentUserContext() {UserId = UserId};
    private readonly IMapper _stubMapper;

    public DynamoDbPayerPayeeRepositoryTests()
    {
        _stubMapper = new MapperConfiguration(cfg =>
                cfg.AddMaps(typeof(DynamoDbPayerPayeeRepository)))
            .CreateMapper();
    }

    public class GetPayeesPaginatedTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>(), 0, 2, new List<PayerPayee>()
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
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
                    }
                },
                0, 2, new List<PayerPayee>()
                {
                    new()
                    {
                        ExternalId = "123",
                        PayerPayeeId = "id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        ExternalId = "234",
                        PayerPayeeId = "id2",
                        PayerPayeeName = "name2"
                    },
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
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
                },
                0, 10, new List<PayerPayee>()
                {
                    new()
                    {
                        ExternalId = "123",
                        PayerPayeeId = "id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        ExternalId = "234",
                        PayerPayeeId = "id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        ExternalId = "345",
                        PayerPayeeId = "id3",
                        PayerPayeeName = "name3"
                    },
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
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
                    }
                },
                2, 2, new List<PayerPayee>
                {
                    new()
                    {
                        ExternalId = "345",
                        PayerPayeeId = "id3",
                        PayerPayeeName = "name3"
                    }
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
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
                    }
                },
                4, 2, new List<PayerPayee>()
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Theory]
    [ClassData(typeof(GetPayeesPaginatedTestData))]
    public async Task GivenPaginationSpec_WhenGetPayeesInvoked_ThenCorrectPayeesReturned(
        List<DynamoDbPayerPayee> payees,
        int offset, int limit, List<PayerPayee> expectedReturnedPayees)
    {
        var repository =
            new DynamoDbPayerPayeeRepository(_stubConfig, _dynamoDbContextMock.Object, _currentUserContext,
                _stubMapper);

        var mockAsyncSearch = new Mock<AsyncSearch<DynamoDbPayerPayee>>();
        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => payees);

        _dynamoDbContextMock.Setup(context => context.QueryAsync<DynamoDbPayerPayee>($"{UserId}#PayersPayees",
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

    public class GetPayersPaginatedTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>(), 0, 2, new List<PayerPayee>()
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
                {
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "123",
                        PayerPayeeId = "payer#id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "234",
                        PayerPayeeId = "payer#id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "345",
                        PayerPayeeId = "payer#id3",
                        PayerPayeeName = "name3"
                    }
                },
                0, 2, new List<PayerPayee>()
                {
                    new()
                    {
                        ExternalId = "123",
                        PayerPayeeId = "id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        ExternalId = "234",
                        PayerPayeeId = "id2",
                        PayerPayeeName = "name2"
                    },
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
                {
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "123",
                        PayerPayeeId = "payer#id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "234",
                        PayerPayeeId = "payer#id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "345",
                        PayerPayeeId = "payer#id3",
                        PayerPayeeName = "name3"
                    },
                },
                0, 10, new List<PayerPayee>()
                {
                    new()
                    {
                        ExternalId = "123",
                        PayerPayeeId = "id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        ExternalId = "234",
                        PayerPayeeId = "id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        ExternalId = "345",
                        PayerPayeeId = "id3",
                        PayerPayeeName = "name3"
                    },
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
                {
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "123",
                        PayerPayeeId = "payer#id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "234",
                        PayerPayeeId = "payer#id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "345",
                        PayerPayeeId = "payer#id3",
                        PayerPayeeName = "name3"
                    }
                },
                2, 2, new List<DynamoDbPayerPayee>
                {
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "345",
                        PayerPayeeId = "id3",
                        PayerPayeeName = "name3"
                    }
                }
            };
            yield return new object[]
            {
                new List<DynamoDbPayerPayee>()
                {
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "123",
                        PayerPayeeId = "payer#id1",
                        PayerPayeeName = "name1"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "234",
                        PayerPayeeId = "payer#id2",
                        PayerPayeeName = "name2"
                    },
                    new()
                    {
                        UserId = $"{UserId}#PayersPayees",
                        ExternalId = "345",
                        PayerPayeeId = "payer#id3",
                        PayerPayeeName = "name3"
                    }
                },
                4, 2, new List<DynamoDbPayerPayee>()
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    [Theory]
    [ClassData(typeof(GetPayeesPaginatedTestData))]
    public async Task GivenPaginationSpec_WhenGetPayersInvoked_ThenCorrectPayersReturned(
        List<DynamoDbPayerPayee> payers,
        int offset, int limit, List<PayerPayee> expectedReturnedPayees)
    {
        var repository = new DynamoDbPayerPayeeRepository(_stubConfig, _dynamoDbContextMock.Object, _currentUserContext,
            _stubMapper);

        var mockAsyncSearch = new Mock<AsyncSearch<DynamoDbPayerPayee>>();
        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => payers);

        _dynamoDbContextMock.Setup(context => context.QueryAsync<DynamoDbPayerPayee>($"{UserId}#PayersPayees",
            QueryOperator.BeginsWith, new[] {"payer#"},
            It.Is<DynamoDBOperationConfig>(
                config => config.OverrideTableName == TableName))).Returns(mockAsyncSearch.Object);

        var returnedPayees = await repository.GetPayers(
            UserId, new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            });

        Assert.Equal(expectedReturnedPayees, returnedPayees);
    }
}