using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Domain.Specifications;
using TransactionService.Dtos;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain
{
    public class TransactionHelperServiceTests
    {
        public class Constructor
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public Constructor()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public void GivenNullCurrentUserContext_ThenArgumentNullExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(null, _mockTransactionRepository.Object, _mockMapper.Object));
            }

            [Fact]
            public void GivenNullITransactionRepository_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(_mockCurrentUserContext.Object, null, _mockMapper.Object));
            }

            [Fact]
            public void GivenNullIMapper_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(_mockCurrentUserContext.Object, _mockTransactionRepository.Object,
                        null));
            }
        }

        public class GetAllTransactionsAsync
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public GetAllTransactionsAsync()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public void
                GivenUserIdInCurrentUserContext_ThenFunctionCalledWithCorrectUserId()
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
            public async Task
                GivenValidInputArguments_ThenReturnsListOfTransactions()
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
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
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
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                    Note = "this is a note123"
                };
                var expectedTransactionList = new List<Transaction>
                {
                    transaction1, transaction2
                };

                _mockTransactionRepository.Setup(repository =>
                        repository.GetAllTransactionsAsync(It.IsAny<string>(), It.IsAny<DateTime>(),
                            It.IsAny<DateTime>()))
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
                GivenNonNullTransactionType_ThenReturnsListOfTransactions()
            {
                const string expectedTransactionType = "expense";
                var transaction1 = new Transaction
                {
                    Amount = (decimal) 1.0,
                    Category = "category-1",
                    TransactionTimestamp = DateTime.Now.ToString("O"),
                    SubCategory = "subcategory-1",
                    TransactionId = "transaction-id-1",
                    TransactionType = "expense",
                    UserId = "userid-1",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
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
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                    Note = "this is a note123"
                };
                var expectedTransactionList = new List<Transaction>
                {
                    transaction1, transaction2
                };

                _mockTransactionRepository.Setup(repository =>
                        repository.GetAllTransactionsAsync(It.IsAny<string>(), It.IsAny<DateTime>(),
                            It.IsAny<DateTime>(),
                            expectedTransactionType))
                    .ReturnsAsync(expectedTransactionList);

                var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                    _mockTransactionRepository.Object, _mockMapper.Object);
                var response =
                    await service.GetAllTransactionsAsync(DateTime.MinValue, DateTime.MaxValue,
                        expectedTransactionType);

                Assert.Equal(2, response.Count);
                Assert.Equal(transaction1, response[0]);
                Assert.Equal(transaction2, response[1]);
            }
        }

        public class GetTransactionsAsync
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public GetTransactionsAsync()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public async Task GivenValidInputs_ThenCorrectTransactionsReturned()
            {
                const string expectedUserId = "id123";

                _mockCurrentUserContext.SetupGet(context => context.UserId)
                    .Returns(expectedUserId);

                var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                    _mockTransactionRepository.Object, _mockMapper.Object);

                var expectedTransactionList = new List<Transaction>()
                {
                    new()
                    {
                        Amount = decimal.One,
                        Category = "test category123",
                    }
                };
                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(expectedUserId,
                        new DateRange(DateTime.MinValue, DateTime.MaxValue), It.IsAny<ITransactionSpecification>()))
                    .ReturnsAsync(() => expectedTransactionList);

                var response = await service.GetTransactionsAsync(new GetTransactionsQuery());
                Assert.Equal(expectedTransactionList, response);
            }

            [Fact]
            public async Task GivenQueryInputs_ThenRepositoryCalledWithCorrectSpecification()
            {
                const string expectedUserId = "id123";
                var expectedTransactionType = TransactionType.Expense;
                _mockCurrentUserContext.SetupGet(context => context.UserId)
                    .Returns(expectedUserId);

                var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                    _mockTransactionRepository.Object, _mockMapper.Object);

                ITransactionSpecification calledWithSpecification = null;
                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(expectedUserId,
                        new DateRange(DateTime.MinValue, DateTime.MaxValue), It.IsAny<ITransactionSpecification>()))
                    .Callback((string _, DateRange _, ITransactionSpecification transactionSpecification) =>
                    {
                        calledWithSpecification = transactionSpecification;
                    });

                await service.GetTransactionsAsync(new GetTransactionsQuery
                {
                    Type = expectedTransactionType
                });

                Assert.IsType<AndSpec>(calledWithSpecification);
                Assert.True(calledWithSpecification.IsSatisfied(new Transaction
                {
                    TransactionType = "expense"
                }));
                Assert.False(calledWithSpecification.IsSatisfied(new Transaction
                {
                    TransactionType = "invalid type"
                }));
            }
        }

        public class StoreTransaction
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public StoreTransaction()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public async Task
                GivenValidStoreTransactionDto_ThenCorrectTransactionShouldBeStored()
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
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
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
                        PayerPayeeId = inputDto.PayerPayeeId,
                        PayerPayeeName = inputDto.PayerPayeeName,
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
                        transaction.PayerPayeeId == inputDto.PayerPayeeId &&
                        transaction.PayerPayeeName == inputDto.PayerPayeeName &&
                        transaction.Note == inputDto.Note))
                );
            }
        }

        public class PutTransaction
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public PutTransaction()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public async Task GivenPutTransactionDto_ThenCorrectTransactionShouldBeUpdated()
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
                    PayerPayeeId = "id123",
                    PayerPayeeName = "name123",
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
                        PayerPayeeId = "id123",
                        PayerPayeeName = "name123",
                        Note = "this is a note123"
                    }
                );

                var service = new TransactionHelperService(_mockCurrentUserContext.Object,
                    _mockTransactionRepository.Object, _mockMapper.Object);

                await service.PutTransaction(expectedTransactionId, new PutTransactionDto());

                _mockTransactionRepository.Verify(repository =>
                    repository.PutTransaction(expectedTransaction));
            }
        }

        public class DeleteTransaction
        {
            private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public DeleteTransaction()
            {
                _mockCurrentUserContext = new Mock<CurrentUserContext>();
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
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
}