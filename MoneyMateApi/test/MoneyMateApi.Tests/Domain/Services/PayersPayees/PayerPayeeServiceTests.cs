using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.Dtos;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.PayerPayees;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories;
using MoneyMateApi.Services.PayerPayeeEnricher;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.PayersPayees;

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
        _mockPayerPayeeEnricher.Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, payers[0]))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = payers[0].ExternalId,
                    PayerPayeeId = Guid.Parse(payers[0].PayerPayeeId),
                    PayerPayeeName = payers[0].PayerPayeeName,
                    Address = addresses[0]
                });
        _mockPayerPayeeEnricher.Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, payers[1]))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = payers[1].ExternalId,
                    PayerPayeeId = Guid.Parse(payers[1].PayerPayeeId),
                    PayerPayeeName = payers[1].PayerPayeeName,
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
    public async Task GivenOffsetAndLimit_WhenGetPayeesInvoked_CorrectIEnumerableReturned()
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
        _mockPayerPayeeEnricher.Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[0]))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = payees[0].ExternalId,
                    PayerPayeeId = Guid.Parse(payees[0].PayerPayeeId),
                    PayerPayeeName = payees[0].PayerPayeeName,
                    Address = addresses[0]
                });
        _mockPayerPayeeEnricher.Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[1]))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = payees[1].ExternalId,
                    PayerPayeeId = Guid.Parse(payees[1].PayerPayeeId),
                    PayerPayeeName = payees[1].PayerPayeeName,
                    Address = addresses[1]
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);

        var response = await service.GetPayees(offset, limit);

        var payerViewModels = payees.Select((payee, index) => new PayerPayeeViewModel
        {
            ExternalId = payee.ExternalId,
            PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
            PayerPayeeName = payee.PayerPayeeName,
            Address = addresses[index]
        });
        Assert.Equal(payerViewModels, response);
    }

    [Fact]
    public async Task GivenIncludeEnrichedDataIsFalse_WhenGetPayersInvoked_CorrectPayerPayeeModelsReturned()
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

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);

        var response = await service.GetPayers(offset, limit, false);

        var payeeViewModels = payers.Select((payer, index) => new PayerPayeeViewModel
        {
            ExternalId = payer.ExternalId,
            PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
            PayerPayeeName = payer.PayerPayeeName,
        });
        Assert.Equal(payeeViewModels, response);
    }

    [Fact]
    public async Task GivenIncludeEnrichedDataIsFalse_WhenGetPayeesInvoked_CorrectPayerPayeeModelsReturned()
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

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);

        var response = await service.GetPayees(offset, limit, false);

        var payeeViewModels = payees.Select((payee, index) => new PayerPayeeViewModel
        {
            ExternalId = payee.ExternalId,
            PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
            PayerPayeeName = payee.PayerPayeeName,
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

        var storedPayer = new PayerPayee
        {
            ExternalId = externalId,
            PayerPayeeId = payerPayeeId.ToString(),
            PayerPayeeName = name
        };
        _mockRepository.Setup(repository => repository.GetPayer(payerPayeeId))
            .ReturnsAsync(() => storedPayer);

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, storedPayer))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = externalId,
                    PayerPayeeId = payerPayeeId,
                    PayerPayeeName = name,
                    Address = expectedAddress
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayer(payerPayeeId);

        Assert.Equal(expectedPayer, actualPayer);
    }

    [Fact]
    public async Task GivenRepositoryResponseWithExternalId_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
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

        var storedPayee = new PayerPayee
        {
            ExternalId = externalId,
            PayerPayeeId = payerPayeeId.ToString(),
            PayerPayeeName = name
        };
        _mockRepository.Setup(repository => repository.GetPayee(payerPayeeId))
            .ReturnsAsync(() => storedPayee);

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, storedPayee))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    ExternalId = externalId,
                    PayerPayeeId = payerPayeeId,
                    PayerPayeeName = name,
                    Address = expectedAddress
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayee(payerPayeeId);

        Assert.Equal(expectedPayee, actualPayer);
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

        var repositoryPayer = new PayerPayee
        {
            PayerPayeeId = payerPayeeId.ToString(),
            PayerPayeeName = name
        };
        _mockRepository.Setup(repository => repository.GetPayer(payerPayeeId))
            .ReturnsAsync(() => repositoryPayer);

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, repositoryPayer))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    PayerPayeeId = payerPayeeId,
                    PayerPayeeName = name,
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayer = await service.GetPayer(payerPayeeId);

        Assert.Equal(expectedPayer, actualPayer);
    }

    [Fact]
    public async Task
        GivenRepositoryResponseWithNoExternalId_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
    {
        var name = "name";
        var payerPayeeId = Guid.NewGuid();

        var expectedPayee = new PayerPayeeViewModel
        {
            PayerPayeeId = payerPayeeId,
            PayerPayeeName = name
        };

        var repositoryPayee = new PayerPayee
        {
            PayerPayeeId = payerPayeeId.ToString(),
            PayerPayeeName = name
        };
        _mockRepository.Setup(repository => repository.GetPayee(payerPayeeId))
            .ReturnsAsync(() => repositoryPayee);

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, repositoryPayee))
            .ReturnsAsync(() =>
                new PayerPayeeViewModel()
                {
                    PayerPayeeId = payerPayeeId,
                    PayerPayeeName = name,
                });

        var service = new PayerPayeeService(_mockRepository.Object,
            _mockPayerPayeeEnricher.Object);
        var actualPayee = await service.GetPayee(payerPayeeId);

        Assert.Equal(expectedPayee, actualPayee);
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

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, repositoryPayers[0]))
            .ReturnsAsync(
                () =>
                    new PayerPayeeViewModel
                    {
                        ExternalId = "externalId1",
                        PayerPayeeId = payerPayeeId1,
                        Address = expectedAddress,
                        PayerPayeeName = "name1"
                    });

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, repositoryPayers[1]))
            .ReturnsAsync(
                () =>
                    new PayerPayeeViewModel
                    {
                        PayerPayeeId = payerPayeeId2,
                        PayerPayeeName = "name2"
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

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, repositoryPayees[0]))
            .ReturnsAsync(
                () =>
                    new PayerPayeeViewModel
                    {
                        ExternalId = "externalId1",
                        PayerPayeeId = payerPayeeId1,
                        Address = expectedAddress,
                        PayerPayeeName = "name1"
                    });

        _mockPayerPayeeEnricher
            .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, repositoryPayees[1]))
            .ReturnsAsync(
                () =>
                    new PayerPayeeViewModel
                    {
                        PayerPayeeId = payerPayeeId2,
                        PayerPayeeName = "name2"
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

    public class GetSuggestedPayersOrPayees
    {
        private readonly Mock<IPayerPayeeRepository> _mockRepository = new();
        private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher = new();

        [Fact]
        public async Task
            GivenAllSuggestionPromptType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedPayersReturned()
        {
            var payers = new List<PayerPayee>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };

            var expectedPayerViewModels = payers.Select(payer => new PayerPayeeViewModel
            {
                PayerPayeeName = payer.PayerPayeeName,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId)
            }).ToList();

            _mockRepository
                .Setup(repo => repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                    new GeneralPayerPayeeSuggestionParameters(), It.IsAny<int>()))
                .ReturnsAsync(() => payers);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, payers[0]))
                .ReturnsAsync(() => expectedPayerViewModels[0]);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, payers[1]))
                .ReturnsAsync(() => expectedPayerViewModels[1]);


            var service = new PayerPayeeService(_mockRepository.Object, _mockPayerPayeeEnricher.Object);

            var suggestedPayers =
                await service.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                    new SuggestionPromptDto(), true);

            Assert.Equal(expectedPayerViewModels, suggestedPayers);
        }

        [Fact]
        public async Task
            GivenSuggestionPromptDtoWithCategoryPromptType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedPayersReturned()
        {
            var payees = new List<PayerPayee>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };

            var expectedPayerViewModels = payees.Select(payee => new PayerPayeeViewModel
            {
                PayerPayeeName = payee.PayerPayeeName,
                PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                ExternalId = "id123",
                Address = "address123"
            }).ToList();

            _mockRepository
                .Setup(repo => repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                    new CategoryPayerPayeeSuggestionParameters("category"), It.IsAny<int>()))
                .ReturnsAsync(() => payees);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[0]))
                .ReturnsAsync(() => expectedPayerViewModels[0]);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[1]))
                .ReturnsAsync(() => expectedPayerViewModels[1]);

            var service = new PayerPayeeService(_mockRepository.Object, _mockPayerPayeeEnricher.Object);

            var suggestedPayers =
                await service.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                    new SuggestionPromptDto
                        { PromptType = SuggestionPromptType.Category, Category = "category" }, true);

            Assert.Equal(expectedPayerViewModels, suggestedPayers);
        }

        [Fact]
        public async Task
            GivenSuggestionPromptDtoWithSubcategoryPromptType_WhenGetSuggestedPayersOrPayeesInvoked_ThenCorrectSuggestedPayersReturned()
        {
            var payees = new List<PayerPayee>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };

            var expectedPayerViewModels = payees.Select(payee => new PayerPayeeViewModel
            {
                PayerPayeeName = payee.PayerPayeeName,
                PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                ExternalId = "id123",
                Address = "address123"
            }).ToList();

            _mockRepository
                .Setup(repo => repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                    new SubcategoryPayerPayeeSuggestionParameters("category", "subcategory1"), It.IsAny<int>()))
                .ReturnsAsync(() => payees);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[0]))
                .ReturnsAsync(() => expectedPayerViewModels[0]);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payee, payees[1]))
                .ReturnsAsync(() => expectedPayerViewModels[1]);

            var service = new PayerPayeeService(_mockRepository.Object, _mockPayerPayeeEnricher.Object);

            var suggestedPayers =
                await service.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                    new SuggestionPromptDto
                    {
                        PromptType = SuggestionPromptType.Subcategory, Category = "category",
                        Subcategory = "subcategory1"
                    }, true);
            Assert.Equal(expectedPayerViewModels, suggestedPayers);
        }

        [Fact]
        public async Task
            GivenIncludeEnrichedDataIsFalse_WhenGetSuggestedPayersOrPayeesInvoked_ThenPayerPayeeReturnedWithoutEnrichedData()
        {
            var payers = new List<PayerPayee>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };

            var expectedPayerViewModels = payers.Select(payer => new PayerPayeeViewModel
            {
                PayerPayeeName = payer.PayerPayeeName,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
            }).ToList();

            _mockRepository
                .Setup(repo => repo.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                    new GeneralPayerPayeeSuggestionParameters(), It.IsAny<int>()))
                .ReturnsAsync(() => payers);

            _mockPayerPayeeEnricher
                .Setup(enricher => enricher.EnrichPayerPayeeToViewModel(PayerPayeeType.Payer, payers[0]))
                .ReturnsAsync(() => expectedPayerViewModels[0]);

            var service = new PayerPayeeService(_mockRepository.Object, _mockPayerPayeeEnricher.Object);

            var suggestedPayers =
                await service.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                    new SuggestionPromptDto());

            Assert.Equal(expectedPayerViewModels, suggestedPayers);
        }
    }

// public class PayerPayeeServiceCreatePayeeTests
// {
//     private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
//     private readonly Mock<IPayerPayeeRepository> _mockRepository;
//     private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher;
//
//     public PayerPayeeServiceCreatePayeeTests()
//     {
//         _mockCurrentUserContext = new Mock<CurrentUserContext>();
//         _mockRepository = new Mock<IPayerPayeeRepository>();
//         _mockPayerPayeeEnricher = new Mock<IPayerPayeeEnricher>();
//     }
//
//     [Fact]
//     public async Task
//         GivenValidCreatePayerPayeeDto_ThenRepositoryCalledWithCorrectPayerPayeeModel()
//     {
//         const string expectedPayeeName = "payee name 123";
//
//         var service = new PayerPayeeService(_mockRepository.Object,
//             _mockPayerPayeeEnricher.Object);
//         await service.CreatePayee(new CreatePayerPayeeDto
//         {
//             Name = expectedPayeeName,
//         });
//
//         _mockRepository.Verify(repository => repository.CreatePayerOrPayee(PayerPayeeType.Payee, It.Is<PayerPayee>(
//             payerPayee =>
//                 payerPayee.ExternalId == "" &&
//                 payerPayee.PayerPayeeName == expectedPayeeName &&
//                 !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
//         )));
//     }
//
//     [Fact]
//     public async Task GivenRepositoryResponse_ThenCorrectPayeeReturned()
//     {
//         const string expectedPayeeName = "payer name 123";
//         const string expectedExternalId = "externalId123";
//         const string expectedAddress = "1 address VIC";
//
//         _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(expectedExternalId))
//             .ReturnsAsync(() => new ExtraPayerPayeeDetails
//             {
//                 Address = expectedAddress
//             });
//
//         var service = new PayerPayeeService(_mockRepository.Object,
//             _mockPayerPayeeEnricher.Object);
//         var actualPayee = await service.CreatePayee(new CreatePayerPayeeDto
//         {
//             Name = expectedPayeeName,
//             ExternalId = expectedExternalId
//         });
//
//         Assert.Equal(expectedExternalId, actualPayee.ExternalId);
//         Assert.Equal(expectedPayeeName, actualPayee.PayerPayeeName);
//         Assert.NotEqual(Guid.Empty, actualPayee.PayerPayeeId);
//         Assert.Equal(expectedAddress, actualPayee.Address);
//     }
// }
//
// public class PayerPayeeServiceCreatePayerTests
// {
//     private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
//     private readonly Mock<IPayerPayeeRepository> _mockRepository;
//     private readonly Mock<IPayerPayeeEnricher> _mockPayerPayeeEnricher;
//
//     public PayerPayeeServiceCreatePayerTests()
//     {
//         _mockCurrentUserContext = new Mock<CurrentUserContext>();
//         _mockRepository = new Mock<IPayerPayeeRepository>();
//         _mockPayerPayeeEnricher = new Mock<IPayerPayeeEnricher>();
//     }
//
//     [Fact]
//     public async Task
//         GivenValidCreatePayerPayeeDto_ThenRepositoryCalledWithCorrectPayerPayeeModel()
//     {
//         const string expectedPayerName = "payer name 123";
//
//         var service = new PayerPayeeService(_mockRepository.Object,
//             _mockPayerPayeeEnricher.Object);
//         await service.CreatePayer(new CreatePayerPayeeDto
//         {
//             Name = expectedPayerName,
//         });
//
//         _mockRepository.Verify(repository => repository.CreatePayerOrPayee(PayerPayeeType.Payer, It.Is<PayerPayee>(
//             payerPayee =>
//                 payerPayee.ExternalId == "" &&
//                 payerPayee.PayerPayeeName == expectedPayerName &&
//                 !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
//         )));
//     }
//
//     [Fact]
//     public async Task GivenPayerCreatedAndExternalIdIsNotEmpty_ThenCorrectPayerReturned()
//     {
//         const string expectedPayerName = "payer name 123";
//         const string expectedExternalId = "externalId123";
//         const string expectedAddress = "address123";
//
//         _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(expectedExternalId))
//             .ReturnsAsync(() => new ExtraPayerPayeeDetails
//             {
//                 Address = expectedAddress
//             });
//
//         var service = new PayerPayeeService(_mockRepository.Object,
//             _mockPayerPayeeEnricher.Object);
//         var actualPayer = await service.CreatePayer(new CreatePayerPayeeDto
//         {
//             Name = expectedPayerName,
//             ExternalId = expectedExternalId
//         });
//
//         Assert.Equal(expectedExternalId, actualPayer.ExternalId);
//         Assert.Equal(expectedPayerName, actualPayer.PayerPayeeName);
//         Assert.NotEqual(Guid.Empty, actualPayer.PayerPayeeId);
//         Assert.Equal(expectedAddress, actualPayer.Address);
//     }
//
//     [Fact]
//     public async Task GivenPayerCreatedAndExternalIdIsEmpty_ThenCorrectPayerReturned()
//     {
//         const string expectedPayerName = "payer name 123";
//
//         var service = new PayerPayeeService(_mockRepository.Object,
//             _mockPayerPayeeEnricher.Object);
//         var actualPayer = await service.CreatePayer(new CreatePayerPayeeDto
//         {
//             Name = expectedPayerName,
//         });
//
//         Assert.Empty(actualPayer.ExternalId);
//         Assert.Equal(expectedPayerName, actualPayer.PayerPayeeName);
//         Assert.NotEqual(Guid.Empty, actualPayer.PayerPayeeId);
//         Assert.Null(actualPayer.Address);
//     }
}