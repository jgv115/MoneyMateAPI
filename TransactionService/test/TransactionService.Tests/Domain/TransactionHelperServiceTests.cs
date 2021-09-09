using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain
{
    public class TransactionHelperServiceTests
    {
        private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<IMapper> _mockMapper;

        public TransactionHelperServiceTests()
        {
            _mockCurrentUserContext = new Mock<CurrentUserContext>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public void GivenNullCurrentUserContext_WhenConstructorInvoked_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TransactionHelperService(null, _mockTransactionRepository.Object, _mockMapper.Object));
        }

        [Fact]
        public void GivenNullITransactionRepository_WhenConstructorInvoked_ThenArgumentNulExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TransactionHelperService(_mockCurrentUserContext.Object, null, _mockMapper.Object));
        }

        [Fact]
        public void GivenNullIMapper_WhenConstructorInvoked_ThenArgumentNulExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new TransactionHelperService(_mockCurrentUserContext.Object, _mockTransactionRepository.Object, null));
        }

        [Fact]
        public void
            GivenUserIdInCurrentUserContext_WhenGetAllTransactionAsyncInvoked_ThenFunctionCalledWithCorrectUserId()
        {
            const string expectedUserId = "id123";

            _mockCurrentUserContext.SetupGet(context => context.UserId)
                .Returns(expectedUserId);

            var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                _mockTransactionRepository.Object, _mockMapper.Object);

            service.GetAllTransactionsAsync(DateTime.MinValue, DateTime.MaxValue);

            _mockTransactionRepository.Verify(repository =>
                repository.GetAllTransactionsAsync(expectedUserId, It.IsAny<DateTime>(), It.IsAny<DateTime>()));
        }

        [Fact]
        public async Task GivenValidInputArguments_WhenGetAllTransactionsAsyncInvoked_ThenReturnsListOfTransactions()
        {
            var transaction1 = new Transaction
            {
                Amount = (decimal) 1.0,
                Category = "category-1",
                TransactionTimestamp = DateTime.Now.ToString("O"),
                SubCategory = "subcategory-1",
                TransactionId = "transaction-id-1",
                TransactionType = "expense",
                UserId = "userid-1",
                Note = "this is a note123"
            };

            var transaction2 = new Transaction
            {
                Amount = (decimal) 2.0,
                Category = "category-2",
                TransactionTimestamp = DateTime.Now.ToString("O"),
                SubCategory = "subcategory-2",
                TransactionId = "transaction-id-2",
                TransactionType = "expense",
                UserId = "userid-2",
                Note = "this is a note123"
            };
            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2
            };

            _mockTransactionRepository.Setup(repository =>
                    repository.GetAllTransactionsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedTransactionList);

            var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                _mockTransactionRepository.Object, _mockMapper.Object);
            var response = await service.GetAllTransactionsAsync(DateTime.MinValue, DateTime.MaxValue);

            Assert.Equal(2, response.Count);
            Assert.Equal(transaction1, response[0]);
            Assert.Equal(transaction2, response[1]);
        }

        [Fact]
        public async Task
            GivenValidStoreTransactionDto_WhenStoreTransactionInvoked_ThenCorrectTransactionShouldBeStored()
        {
            const string expectedUserId = "id123";

            _mockCurrentUserContext.SetupGet(context => context.UserId)
                .Returns(expectedUserId);

            var inputDto = new StoreTransactionDto
            {
                Amount = (decimal) 1.0,
                TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                Category = "category-1",
                SubCategory = "subcategory-1",
                TransactionType = "transaction-type-1",
                Note = "this is a note123"
            };

            _mockMapper.Setup(mapper => mapper.Map<Transaction>(It.IsAny<StoreTransactionDto>()))
                .Returns(new Transaction
                {
                    Amount = inputDto.Amount,
                    TransactionTimestamp = inputDto.TransactionTimestamp,
                    Category = inputDto.Category,
                    SubCategory = inputDto.SubCategory,
                    TransactionType = inputDto.TransactionType,
                    Note = inputDto.Note
                });

            var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                _mockTransactionRepository.Object, _mockMapper.Object);

            await service.StoreTransaction(inputDto);

            Guid guid;
            _mockTransactionRepository.Verify(repository =>
                repository.StoreTransaction(It.Is<Transaction>(transaction =>
                    transaction.UserId == expectedUserId &&
                    transaction.TransactionTimestamp == inputDto.TransactionTimestamp &&
                    Guid.TryParse(transaction.TransactionId, out guid) &&
                    transaction.Amount == inputDto.Amount &&
                    transaction.Category == inputDto.Category &&
                    transaction.SubCategory == inputDto.SubCategory &&
                    transaction.TransactionType == inputDto.TransactionType &&
                    transaction.Note == inputDto.Note))
            );
        }

        [Fact]
        public async Task GivenPutTransactionDto_WhenPutTransactionInvoked_ThenCorrectTransactionShouldBeUpdated()
        {
            const string expectedUserId = "id123";
            var expectedTransactionId = Guid.NewGuid().ToString();

            _mockCurrentUserContext.SetupGet(context => context.UserId)
                .Returns(expectedUserId);

            var expectedTransaction = new Transaction
            {
                UserId = expectedUserId,
                TransactionId = expectedTransactionId,
                Amount = (decimal) 1.0,
                TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                Category = "category-1",
                SubCategory = "subcategory-1",
                TransactionType = "type",
                Note = "this is a note123"
            };

            _mockMapper.Setup(mapper => mapper.Map<Transaction>(It.IsAny<PutTransactionDto>())).Returns(
                new Transaction
                {
                    Amount = (decimal) 1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    SubCategory = "subcategory-1",
                    TransactionType = "type",
                    Note = "this is a note123"
                }
            );

            var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                _mockTransactionRepository.Object, _mockMapper.Object);

            await service.PutTransaction(expectedTransactionId, new PutTransactionDto());

            _mockTransactionRepository.Verify(repository =>
                repository.PutTransaction(expectedTransaction));
        }

        [Fact]
        public async Task
            GivenTransactionId_WhenDeleteTransactionInvoked_ThenRepositoryDeleteTransactionCalledWithCorrectArgument()
        {
            const string expectedUserId = "id123";
            var expectedTransactionId = Guid.NewGuid().ToString();

            _mockCurrentUserContext.SetupGet(context => context.UserId)
                .Returns(expectedUserId);

            var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                _mockTransactionRepository.Object, _mockMapper.Object);
            await service.DeleteTransaction(expectedTransactionId);

            _mockTransactionRepository.Verify(repository =>
                repository.DeleteTransaction(expectedUserId, expectedTransactionId));
        }
    }
}