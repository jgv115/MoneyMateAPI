using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.PayerPayees;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Models;
using Xunit;

namespace TransactionService.Tests.Domain.Services;

public class PayerPayeeServiceTests
{
    private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
    private readonly Mock<IPayerPayeeRepository> _mockRepository;
    private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher;

    public PayerPayeeServiceTests()
    {
        _mockPayerPayeeEnricher = new Mock<IPayerPayeeEnricher>();
        _mockCurrentUserContext = new Mock<CurrentUserContext>();
        _mockRepository = new Mock<IPayerPayeeRepository>();
    }

    [Fact]
    public void GivenNullRepository_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PayerPayeeService(null, _mockPayerPayeeEnricher.Object));
    }

    [Fact]
    public async Task GivenOffsetAndLimit_WhenGetPayersInvoked_CorrectIEnumerableReturned()
    {
        const int limit = 25;
        const int offset = 2;
        var payers = new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name123",
                ExternalId = "id123"
            },
            new()
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name123",
                ExternalId = "id1234"
            }
        };
        _mockRepository.Setup(repository => repository.GetPayers(new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            }))
            .ReturnsAsync(() => payers);

        var addresses = new[]
        {
            "1 address",
            "2 address"
        };
        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("id123")).ReturnsAsync(() =>
            new ExtraPayerPayeeDetails
            {
                Address = addresses[0]
            });
        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("id1234")).ReturnsAsync(() =>
            new ExtraPayerPayeeDetails
            {
                Address = addresses[1]
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);

        var response = await service.GetPayers(offset, limit);

        var payerViewModels = payers.Select((payer, index) => new PayerPayeeViewModel
        {
            ExternalId = payer.ExternalId,
            PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
            PayerPayeeName = payer.PayerPayeeName,
            Address = addresses[index]
        });
        Assert.Equal(payerViewModels, response);
    }

    [Fact]
    public async Task GivenOffsetAndLimit_WhenGetPayeesInvoked_CorrectPayerPayeeModelsReturned()
    {
        const int limit = 25;
        const int offset = 2;
        var payees = new List<PayerPayee>
        {
            new()
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name123",
                ExternalId = "id123"
            },
            new()
            {
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name123",
                ExternalId = "id1234"
            }
        };
        _mockRepository.Setup(repository => repository.GetPayees(new PaginationSpec
            {
                Limit = limit,
                Offset = offset
            }))
            .ReturnsAsync(() => payees);

        var addresses = new[]
        {
            "1 address",
            "2 address"
        };
        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("id123")).ReturnsAsync(() =>
            new ExtraPayerPayeeDetails
            {
                Address = addresses[0]
            });
        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("id1234")).ReturnsAsync(() =>
            new ExtraPayerPayeeDetails
            {
                Address = addresses[1]
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);

        var response = await service.GetPayees(offset, limit);

        var payeeViewModels = payees.Select((payee, index) => new PayerPayeeViewModel
        {
            ExternalId = payee.ExternalId,
            PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
            PayerPayeeName = payee.PayerPayeeName,
            Address = addresses[index]
        });
        Assert.Equal(payeeViewModels, response);
    }

    [Fact]
    public async Task GivenRepositoryResponseWithExternalId_WhenGetPayerInvoked_ThenCorrectPayerPayeeModelReturned()
    {
        var name = "name";
        var externalId = "externalId";
        var payerPayeeId = Guid.NewGuid();
        const string expectedAddress = "1 test address 3124";
        var expectedPayer = new PayerPayeeViewModel
        {
            ExternalId = externalId,
            PayerPayeeId = payerPayeeId,
            PayerPayeeName = name,
            Address = expectedAddress
        };

        _mockRepository.Setup(repository => repository.GetPayer( payerPayeeId))
            .ReturnsAsync(() => new PayerPayee
            {
                ExternalId = externalId,
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = name
            });

        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(externalId))
            .ReturnsAsync(() => new ExtraPayerPayeeDetails
            {
                Address = expectedAddress
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayer(payerPayeeId);

        Assert.Equal(expectedPayer, actualPayer);
    }

    [Fact]
    public async Task
        GivenRepositoryResponseWithNoExternalId_WhenGetPayerInvoked_ThenCorrectPayerPayeeModelReturned()
    {
        var name = "name";
        var payerPayeeId = Guid.NewGuid();

        var expectedPayer = new PayerPayeeViewModel
        {
            PayerPayeeId = payerPayeeId,
            PayerPayeeName = name
        };

        _mockRepository.Setup(repository => repository.GetPayer(payerPayeeId))
            .ReturnsAsync(() => new PayerPayee
            {
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = name
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayer(payerPayeeId);

        Assert.Equal(expectedPayer, actualPayer);
    }

    [Fact]
    public async Task GivenRepositoryResponse_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
    {
        var name = "name";
        var externalId = "externalId";
        var payerPayeeId = Guid.NewGuid();
        const string expectedAddress = "1 test address 3124";
        var expectedPayee = new PayerPayeeViewModel
        {
            ExternalId = externalId,
            PayerPayeeId = payerPayeeId,
            PayerPayeeName = name,
            Address = expectedAddress
        };

        _mockRepository.Setup(repository => repository.GetPayee(payerPayeeId))
            .ReturnsAsync(() => new PayerPayee
            {
                ExternalId = externalId,
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = name
            });

        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(externalId)).ReturnsAsync(() =>
            new ExtraPayerPayeeDetails
            {
                Address = expectedAddress
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayee(payerPayeeId);

        Assert.Equal(expectedPayee, actualPayer);
    }

    [Fact]
    public async Task
        GivenRepositoryResponseWithNoExternalId_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
    {
        var name = "name";
        var payerPayeeId = Guid.NewGuid();

        var expectedPayer = new PayerPayeeViewModel
        {
            PayerPayeeId = payerPayeeId,
            PayerPayeeName = name
        };

        _mockRepository.Setup(repository => repository.GetPayee( payerPayeeId))
            .ReturnsAsync(() => new PayerPayee
            {
                PayerPayeeId = payerPayeeId.ToString(),
                PayerPayeeName = name
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayee(payerPayeeId);

        Assert.Equal(expectedPayer, actualPayer);
    }

    [Fact]
    public async Task GivenRepositoryResponse_WhenAutocompletePayerInvoked_ThenCorrectEnrichedPayerPayeesReturned()
    {
        var payerName = "test name";
        const string expectedAddress = "1 expected address street vic";
        var payerPayeeId1 = Guid.Parse("4c88ca6d-13f7-4b50-8ba9-bfea3a112f98");
        var payerPayeeId2 = Guid.Parse("6c3e3ec4-eb7a-411e-addb-7c779fd39cbb");

        var repositoryPayers = new List<PayerPayee>
        {
            new()
            {
                ExternalId = "externalId1",
                PayerPayeeId = payerPayeeId1.ToString(),
                PayerPayeeName = "name1"
            },
            new()
            {
                PayerPayeeId = payerPayeeId2.ToString(),
                PayerPayeeName = "name2"
            }
        };
        _mockRepository.Setup(repository => repository.AutocompletePayer(payerName))
            .ReturnsAsync(() => repositoryPayers);

        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("externalId1")).ReturnsAsync(
            () =>
                new ExtraPayerPayeeDetails
                {
                    Address = expectedAddress
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayers = await service.AutocompletePayer(payerName);
        var expectedPayers = new List<PayerPayeeViewModel>
        {
            new()
            {
                ExternalId = "externalId1",
                PayerPayeeId = payerPayeeId1,
                Address = expectedAddress,
                PayerPayeeName = "name1"
            },
            new()
            {
                PayerPayeeId = payerPayeeId2,
                PayerPayeeName = "name2"
            }
        };
        Assert.Equal(expectedPayers, actualPayers);
    }

    [Fact]
    public async Task GivenRepositoryResponse_WhenAutocompletePayeeInvoked_ThenCorrectPayerPayeeEnumerableReturned()
    {
        var payeeName = "test name";
        const string expectedAddress = "1 expected address street vic";
        var payerPayeeId1 = Guid.Parse("4c88ca6d-13f7-4b50-8ba9-bfea3a112f98");
        var payerPayeeId2 = Guid.Parse("6c3e3ec4-eb7a-411e-addb-7c779fd39cbb");

        var repositoryPayees = new List<PayerPayee>
        {
            new()
            {
                ExternalId = "externalId1",
                PayerPayeeId = payerPayeeId1.ToString(),
                PayerPayeeName = "name1"
            },
            new()
            {
                PayerPayeeId = payerPayeeId2.ToString(),
                PayerPayeeName = "name2"
            }
        };
        _mockRepository.Setup(repository => repository.AutocompletePayee(payeeName))
            .ReturnsAsync(() => repositoryPayees);

        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails("externalId1")).ReturnsAsync(
            () =>
                new ExtraPayerPayeeDetails
                {
                    Address = expectedAddress
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayees = await service.AutocompletePayee(payeeName);
        var expectedPayees = new List<PayerPayeeViewModel>
        {
            new()
            {
                ExternalId = "externalId1",
                PayerPayeeId = payerPayeeId1,
                Address = expectedAddress,
                PayerPayeeName = "name1"
            },
            new()
            {
                PayerPayeeId = payerPayeeId2,
                PayerPayeeName = "name2"
            }
        };
        Assert.Equal(expectedPayees, actualPayees);
    }
}

public class PayerPayeeServiceCreatePayeeTests
{
    private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
    private readonly Mock<IPayerPayeeRepository> _mockRepository;
    private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher;

    public PayerPayeeServiceCreatePayeeTests()
    {
        _mockCurrentUserContext = new Mock<CurrentUserContext>();
        _mockRepository = new Mock<IPayerPayeeRepository>();
        _mockPayerPayeeEnricher = new Mock<IPayerPayeeEnricher>();
    }

    [Fact]
    public async Task
        GivenValidCreatePayerPayeeDto_ThenRepositoryCalledWithCorrectPayerPayeeModel()
    {
        const string expectedPayeeName = "payee name 123";
        
        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        await service.CreatePayee(new CreatePayerPayeeDto
        {
            Name = expectedPayeeName,
        });

        _mockRepository.Verify(repository => repository.CreatePayee(It.Is<PayerPayee>(payerPayee =>
            payerPayee.ExternalId == "" &&
            payerPayee.PayerPayeeName == expectedPayeeName &&
            !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
        )));
    }

    [Fact]
    public async Task GivenRepositoryResponse_ThenCorrectPayeeReturned()
    {
        const string expectedPayeeName = "payer name 123";
        const string expectedExternalId = "externalId123";
        const string expectedAddress = "1 address VIC";
        
        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(expectedExternalId))
            .ReturnsAsync(() => new ExtraPayerPayeeDetails
            {
                Address = expectedAddress
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayee = await service.CreatePayee(new CreatePayerPayeeDto
        {
            Name = expectedPayeeName,
            ExternalId = expectedExternalId
        });

        Assert.Equal(expectedExternalId, actualPayee.ExternalId);
        Assert.Equal(expectedPayeeName, actualPayee.PayerPayeeName);
        Assert.NotEqual(Guid.Empty, actualPayee.PayerPayeeId);
        Assert.Equal(expectedAddress, actualPayee.Address);
    }
}

public class PayerPayeeServiceCreatePayerTests
{
    private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
    private readonly Mock<IPayerPayeeRepository> _mockRepository;
    private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher;

    public PayerPayeeServiceCreatePayerTests()
    {
        _mockCurrentUserContext = new Mock<CurrentUserContext>();
        _mockRepository = new Mock<IPayerPayeeRepository>();
        _mockPayerPayeeEnricher = new Mock<IPayerPayeeEnricher>();
    }

    [Fact]
    public async Task
        GivenValidCreatePayerPayeeDto_ThenRepositoryCalledWithCorrectPayerPayeeModel()
    {
        const string expectedPayerName = "payer name 123";

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        await service.CreatePayer(new CreatePayerPayeeDto
        {
            Name = expectedPayerName,
        });

        _mockRepository.Verify(repository => repository.CreatePayer(It.Is<PayerPayee>(payerPayee =>
            payerPayee.ExternalId == "" &&
            payerPayee.PayerPayeeName == expectedPayerName &&
            !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
        )));
    }

    [Fact]
    public async Task GivenPayerCreatedAndExternalIdIsNotEmpty_ThenCorrectPayerReturned()
    {
        const string expectedPayerName = "payer name 123";
        const string expectedExternalId = "externalId123";
        const string expectedAddress = "address123";

        _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(expectedExternalId))
            .ReturnsAsync(() => new ExtraPayerPayeeDetails
            {
                Address = expectedAddress
            });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.CreatePayer(new CreatePayerPayeeDto
        {
            Name = expectedPayerName,
            ExternalId = expectedExternalId
        });

        Assert.Equal(expectedExternalId, actualPayer.ExternalId);
        Assert.Equal(expectedPayerName, actualPayer.PayerPayeeName);
        Assert.NotEqual(Guid.Empty, actualPayer.PayerPayeeId);
        Assert.Equal(expectedAddress, actualPayer.Address);
    }

    [Fact]
    public async Task GivenPayerCreatedAndExternalIdIsEmpty_ThenCorrectPayerReturned()
    {
        const string expectedPayerName = "payer name 123";
        
        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.CreatePayer(new CreatePayerPayeeDto
        {
            Name = expectedPayerName,
        });

        Assert.Empty(actualPayer.ExternalId);
        Assert.Equal(expectedPayerName, actualPayer.PayerPayeeName);
        Assert.NotEqual(Guid.Empty, actualPayer.PayerPayeeId);
        Assert.Null(actualPayer.Address);
    }
}