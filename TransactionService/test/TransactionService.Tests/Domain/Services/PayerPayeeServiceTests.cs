using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services.PayerPayeeEnricher;
using TransactionService.Services.PayerPayeeEnricher.Models;
using TransactionService.ViewModels;
using Xunit;

namespace TransactionService.Tests.Domain.Services
{
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
        public void GivenNullUserContext_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PayerPayeeService(null, _mockRepository.Object, _mockPayerPayeeEnricher.Object));
        }

        [Fact]
        public void GivenNullRepository_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PayerPayeeService(_mockCurrentUserContext.Object, null, _mockPayerPayeeEnricher.Object));
        }

        [Fact]
        public async Task GivenValidUserContext_WhenGetPayersInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.GetPayers();

            _mockRepository.Verify(repository => repository.GetPayers(userId));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenGetPayersInvoked_CorrectIEnumerableReturned()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var payers = new List<PayerPayee>
            {
                new()
                {
                    UserId = "userId123",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name123",
                    ExternalId = "id123"
                },
                new()
                {
                    UserId = "userId1234",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name123",
                    ExternalId = "id1234"
                }
            };

            var payerViewModels = payers.Select(payer => new PayerPayeeViewModel
            {
                ExternalId = payer.ExternalId,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                PayerPayeeName = payer.PayerPayeeName
            });

            _mockRepository.Setup(repository => repository.GetPayers(It.IsAny<string>()))
                .ReturnsAsync(() => payers);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);

            var response = await service.GetPayers();
            Assert.Equal(payerViewModels, response);
        }

        [Fact]
        public async Task GivenValidUserContext_WhenGetPayeesInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.GetPayees();

            _mockRepository.Verify(repository => repository.GetPayees(userId));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenGetPayeesInvoked_CorrectIEnumerableReturned()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var payees = new List<PayerPayee>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    UserId = "userId123",
                    PayerPayeeName = "name123",
                    ExternalId = "id123"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    UserId = "userId1234",
                    PayerPayeeName = "name123",
                    ExternalId = "id1234"
                }
            };

            var payeeViewModels = payees.Select(payee => new PayerPayeeViewModel
            {
                ExternalId = payee.ExternalId,
                PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                PayerPayeeName = payee.PayerPayeeName
            });
            _mockRepository.Setup(repository => repository.GetPayees(It.IsAny<string>()))
                .ReturnsAsync(() => payees);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);

            var response = await service.GetPayees();
            Assert.Equal(payeeViewModels, response);
        }

        [Fact]
        public async Task GivenRepositoryResponseWithExternalId_WhenGetPayerInvoked_ThenCorrectPayerPayeeModelReturned()
        {
            var name = "name";
            var externalId = "externalId";
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            const string expectedAddress = "1 test address 3124";
            var expectedPayer = new PayerPayeeViewModel
            {
                ExternalId = externalId,
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = name,
                Address = expectedAddress
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayer(userId, payerPayeeId))
                .ReturnsAsync(() => new PayerPayee
                {
                    ExternalId = externalId,
                    UserId = userId,
                    PayerPayeeId = payerPayeeId.ToString(),
                    PayerPayeeName = name
                });

            _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(externalId))
                .ReturnsAsync(() => new ExtraPayerPayeeDetails
                {
                    Address = expectedAddress
                });

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayer = await service.GetPayer(payerPayeeId);

            Assert.Equal(expectedPayer, actualPayer);
        }

        [Fact]
        public async Task GivenRepositoryResponseWithNoExternalId_WhenGetPayerInvoked_ThenCorrectPayerPayeeModelReturned()
        {
            var name = "name";
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            
            var expectedPayer = new PayerPayeeViewModel
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = name
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayer(userId, payerPayeeId))
                .ReturnsAsync(() => new PayerPayee
                {
                    UserId = userId,
                    PayerPayeeId = payerPayeeId.ToString(),
                    PayerPayeeName = name
                });

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
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
            var userId = Guid.NewGuid().ToString();
            const string expectedAddress = "1 test address 3124";
            var expectedPayee = new PayerPayeeViewModel
            {
                ExternalId = externalId,
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = name,
                Address = expectedAddress
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayee(userId, payerPayeeId))
                .ReturnsAsync(() => new PayerPayee
                {
                    ExternalId = externalId,
                    UserId = userId,
                    PayerPayeeId = payerPayeeId.ToString(),
                    PayerPayeeName = name
                });

            _mockPayerPayeeEnricher.Setup(enricher => enricher.GetExtraPayerPayeeDetails(externalId)).ReturnsAsync(() =>
                new ExtraPayerPayeeDetails
                {
                    Address = expectedAddress
                });

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayer = await service.GetPayee(payerPayeeId);

            Assert.Equal(expectedPayee, actualPayer);
        }

        [Fact]
        public async Task GivenRepositoryResponseWithNoExternalId_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
        {
            var name = "name";
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            
            var expectedPayer = new PayerPayeeViewModel
            {
                PayerPayeeId = payerPayeeId,
                PayerPayeeName = name
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayee(userId, payerPayeeId))
                .ReturnsAsync(() => new PayerPayee
                {
                    UserId = userId,
                    PayerPayeeId = payerPayeeId.ToString(),
                    PayerPayeeName = name
                });

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayer = await service.GetPayee(payerPayeeId);

            Assert.Equal(expectedPayer, actualPayer);
        }
        
        [Fact]
        public async Task
            GivenValidPayerNameAndUserContext_WhenAutocompletePayerInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var payerName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.AutocompletePayer(payerName);

            _mockRepository.Verify(repository => repository.AutocompletePayer(userId, payerName));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenAutocompletePayerInvoked_ThenCorrectPayerPayeeEnumerableReturned()
        {
            var payerName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var repositoryPayers = new List<PayerPayee>
            {
                new()
                {
                    ExternalId = "externalId1",
                    UserId = "userId",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1"
                },
                new()
                {
                    ExternalId = "externalId2",
                    UserId = "userId",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };
            _mockRepository.Setup(repository => repository.AutocompletePayer(userId, payerName))
                .ReturnsAsync(() => repositoryPayers);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayers = await service.AutocompletePayer(payerName);
            var expectedPayers = repositoryPayers.Select(payer => new PayerPayeeViewModel
            {
                ExternalId = payer.ExternalId,
                PayerPayeeId = Guid.Parse(payer.PayerPayeeId),
                PayerPayeeName = payer.PayerPayeeName
            });
            Assert.Equal(expectedPayers, actualPayers);
        }

        [Fact]
        public async Task
            GivenValidPayerNameAndUserContext_WhenAutocompletePayeeInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var payeeName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.AutocompletePayee(payeeName);

            _mockRepository.Verify(repository => repository.AutocompletePayee(userId, payeeName));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenAutocompletePayeeInvoked_ThenCorrectPayerPayeeEnumerableReturned()
        {
            var payeeName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var repositoryPayees = new List<PayerPayee>
            {
                new()
                {
                    ExternalId = "externalId1",
                    UserId = "userId",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1"
                },
                new()
                {
                    ExternalId = "externalId2",
                    UserId = "userId",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name2"
                }
            };
            _mockRepository.Setup(repository => repository.AutocompletePayee(userId, payeeName))
                .ReturnsAsync(() => repositoryPayees);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayees = await service.AutocompletePayee(payeeName);
            var expectedPayees = repositoryPayees.Select(payee => new PayerPayeeViewModel
            {
                ExternalId = payee.ExternalId,
                PayerPayeeId = Guid.Parse(payee.PayerPayeeId),
                PayerPayeeName = payee.PayerPayeeName
            });
            Assert.Equal(expectedPayees, actualPayees);
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
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.CreatePayer(new CreatePayerPayeeDto
            {
                Name = expectedPayerName,
                ExternalId = expectedExternalId
            });

            _mockRepository.Verify(repository => repository.CreatePayer(It.Is<PayerPayee>(payerPayee =>
                payerPayee.UserId == userId &&
                payerPayee.ExternalId == expectedExternalId &&
                payerPayee.PayerPayeeName == expectedPayerName &&
                !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
            )));
        }

        [Fact]
        public async Task GivenRepositoryResponse_ThenCorrectPayerReturned()
        {
            const string expectedPayerName = "payer name 123";
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayer = await service.CreatePayer(new CreatePayerPayeeDto
            {
                Name = expectedPayerName,
                ExternalId = expectedExternalId
            });

            Assert.Equal(expectedExternalId, actualPayer.ExternalId);
            Assert.Equal(expectedPayerName, actualPayer.PayerPayeeName);
            Assert.NotEqual(Guid.Empty, actualPayer.PayerPayeeId);
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
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            await service.CreatePayee(new CreatePayerPayeeDto
            {
                Name = expectedPayeeName,
                ExternalId = expectedExternalId
            });

            _mockRepository.Verify(repository => repository.CreatePayee(It.Is<PayerPayee>(payerPayee =>
                payerPayee.UserId == userId &&
                payerPayee.ExternalId == expectedExternalId &&
                payerPayee.PayerPayeeName == expectedPayeeName &&
                !Guid.Parse(payerPayee.PayerPayeeId).Equals(Guid.Empty)
            )));
        }

        [Fact]
        public async Task GivenRepositoryResponse_ThenCorrectPayeeReturned()
        {
            const string expectedPayeeName = "payer name 123";
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object,
                _mockPayerPayeeEnricher.Object);
            var actualPayee = await service.CreatePayee(new CreatePayerPayeeDto
            {
                Name = expectedPayeeName,
                ExternalId = expectedExternalId
            });

            Assert.Equal(expectedExternalId, actualPayee.ExternalId);
            Assert.Equal(expectedPayeeName, actualPayee.PayerPayeeName);
            Assert.NotEqual(Guid.Empty, actualPayee.PayerPayeeId);
        }
    }
}