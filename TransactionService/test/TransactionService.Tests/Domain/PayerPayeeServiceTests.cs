using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain;
using TransactionService.Dtos;
using TransactionService.Models;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain
{
    public class PayerPayeeServiceTests
    {
        private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
        private readonly Mock<IPayerPayeeRepository> _mockRepository;

        public PayerPayeeServiceTests()
        {
            _mockCurrentUserContext = new Mock<CurrentUserContext>();
            _mockRepository = new Mock<IPayerPayeeRepository>();
        }

        [Fact]
        public void GivenNullUserContext_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new PayerPayeeService(null, _mockRepository.Object));
        }

        [Fact]
        public void GivenNullRepository_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new PayerPayeeService(_mockCurrentUserContext.Object, null));
        }

        [Fact]
        public void GivenValidUserContext_WhenGetPayersInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            service.GetPayers();

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
                    PayerPayeeId = "test123",
                    PayerPayeeName = "name123",
                    ExternalId = "id123"
                },
                new()
                {
                    UserId = "userId1234",
                    PayerPayeeId = "test1234",
                    PayerPayeeName = "name123",
                    ExternalId = "id1234"
                }
            };
            _mockRepository.Setup(repository => repository.GetPayers(It.IsAny<string>()))
                .ReturnsAsync(() => payers);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);

            var response = await service.GetPayers();
            Assert.Equal(payers, response);
        }

        [Fact]
        public void GivenValidUserContext_WhenGetPayeesInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            service.GetPayees();

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
                    PayerPayeeId = "test123",
                    UserId = "userId123",
                    PayerPayeeName = "name123",
                    ExternalId = "id123"
                },
                new()
                {
                    PayerPayeeId = "test1234",
                    UserId = "userId1234",
                    PayerPayeeName = "name123",
                    ExternalId = "id1234"
                }
            };
            _mockRepository.Setup(repository => repository.GetPayees(It.IsAny<string>()))
                .ReturnsAsync(() => payees);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);

            var response = await service.GetPayees();
            Assert.Equal(payees, response);
        }

        [Fact]
        public async Task GivenValidIdAndUserContext_WhenGetPayerInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            await service.GetPayer(payerPayeeId);

            _mockRepository.Verify(repository => repository.GetPayer(userId, payerPayeeId));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenGetPayerInvoked_ThenCorrectPayerPayeeModelReturned()
        {
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var expectedPayer = new PayerPayee
            {
                ExternalId = "externalId",
                UserId = "userId",
                PayerPayeeId = "payerpayeeId",
                PayerPayeeName = "name"
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayer(userId, payerPayeeId))
                .ReturnsAsync(() => expectedPayer);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            var actualPayer = await service.GetPayer(payerPayeeId);
            
            Assert.Equal(expectedPayer, actualPayer);
        }
        
        [Fact]
        public async Task GivenValidIdAndUserContext_WhenGetPayeeInvoked_ThenRepositoryCalledWithCorrectArguments()
        {
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            await service.GetPayee(payerPayeeId);

            _mockRepository.Verify(repository => repository.GetPayee(userId, payerPayeeId));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenGetPayeeInvoked_ThenCorrectPayerPayeeModelReturned()
        {
            var payerPayeeId = Guid.NewGuid();
            var userId = Guid.NewGuid().ToString();
            var expectedPayee = new PayerPayee
            {
                ExternalId = "externalId",
                UserId = "userId",
                PayerPayeeId = "payerpayeeId",
                PayerPayeeName = "name"
            };
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            _mockRepository.Setup(repository => repository.GetPayee(userId, payerPayeeId))
                .ReturnsAsync(() => expectedPayee);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            var actualPayer = await service.GetPayee(payerPayeeId);
            
            Assert.Equal(expectedPayee, actualPayer);
        }

        [Fact]
        public async Task
            GivenValidPayerNameAndUserContext_WhenAutocompletePayerInvoked_ThenRepositoryCalledWithCorectArguments()
        {
            var payerName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            await service.AutocompletePayer(payerName);
            
            _mockRepository.Verify(repository => repository.AutocompletePayer(userId, payerName));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenAutocompletePayerInvoked_ThenCorrectPayerPayeeEnumerableReturned()
        {
            var payerName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var expectedPayers = new List<PayerPayee>
            {
                new()
                {
                    ExternalId = "externalId1",
                    UserId = "userId",
                    PayerPayeeId = "payerpayeeId1",
                    PayerPayeeName = "name1"
                },
                new()
                {
                    ExternalId = "externalId2",
                    UserId = "userId",
                    PayerPayeeId = "payerpayeeId2",
                    PayerPayeeName = "name2"
                }
            };
            _mockRepository.Setup(repository => repository.AutocompletePayer(userId, payerName))
                .ReturnsAsync(() => expectedPayers);
            
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            var actualPayers = await service.AutocompletePayer(payerName);
            
            Assert.Equal(expectedPayers, actualPayers);
        }
        
        [Fact]
        public async Task
            GivenValidPayerNameAndUserContext_WhenAutocompletePayeeInvoked_ThenRepositoryCalledWithCorectArguments()
        {
            var payeeName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            await service.AutocompletePayee(payeeName);
            
            _mockRepository.Verify(repository => repository.AutocompletePayee(userId, payeeName));
        }

        [Fact]
        public async Task GivenRepositoryResponse_WhenAutocompletePayeeInvoked_ThenCorrectPayerPayeeEnumerableReturned()
        {
            var payeeName = "test name";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var expectedPayees = new List<PayerPayee>
            {
                new()
                {
                    ExternalId = "externalId1",
                    UserId = "userId",
                    PayerPayeeId = "payerpayeeId1",
                    PayerPayeeName = "name1"
                },
                new()
                {
                    ExternalId = "externalId2",
                    UserId = "userId",
                    PayerPayeeId = "payerpayeeId2",
                    PayerPayeeName = "name2"
                }
            };
            _mockRepository.Setup(repository => repository.AutocompletePayee(userId, payeeName))
                .ReturnsAsync(() => expectedPayees);
            
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
            var actualPayees = await service.AutocompletePayee(payeeName);
            
            Assert.Equal(expectedPayees, actualPayees);
        }

        [Fact]
        public async Task
            GivenValidCreatePayerPayeeDto_WhenCreatePayerInvoked_ThenRepositoryCalledWithCorrectPayerPayeeModel()
        {
            const string expectedPayerName = "payer name 123";
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
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
        public async Task
            GivenValidCreatePayerPayeeDto_WhenCreatePayeeInvoked_ThenRepositoryCalledWithCorrectPayerPayeeModel()
        {
            const string expectedPayeeName = "payee name 123";
            const string expectedExternalId = "externalId123";
            var userId = Guid.NewGuid().ToString();
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(userId);

            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);
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
    }
}