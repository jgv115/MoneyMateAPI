using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain;
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

            var payers = new List<PayerPayee>()
            {
                new()
                {
                    Name = "test123",
                    UserId = "userId123",
                    GooglePlaceId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    GooglePlaceId = "id1234"
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

            var payees = new List<PayerPayee>()
            {
                new()
                {
                    Name = "test123",
                    UserId = "userId123",
                    GooglePlaceId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    GooglePlaceId = "id1234"
                }
            };
            _mockRepository.Setup(repository => repository.GetPayees(It.IsAny<string>()))
                .ReturnsAsync(() => payees);
            var service = new PayerPayeeService(_mockCurrentUserContext.Object, _mockRepository.Object);

            var response = await service.GetPayees();
            Assert.Equal(payees, response);
        }
    }
}