using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TransactionService.Constants;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;
using TransactionService.Repositories.DynamoDb.Models;
using Xunit;

namespace TransactionService.Tests.Domain.Services
{
    public class TransactionHelperServiceTests
    {
        public class Constructor
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public Constructor()
            {
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public void GivenNullITransactionRepository_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(null, _mockMapper.Object));
            }

            [Fact]
            public void GivenNullIMapper_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(_mockTransactionRepository.Object,
                        null));
            }
        }

        public class GetTransactionById
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<IMapper> _mockMapper = new();

            [Fact]
            public async Task GivenTransactionId_ThenTransactionFromRepositoryReturned()
            {
                var service = new TransactionHelperService(_mockTransactionRepository.Object, _mockMapper.Object);

                var expectedTransaction = new Transaction
                {
                    Amount = (decimal) 1.0,
                    Category = "category-1",
                    TransactionTimestamp = DateTime.Now.ToString("O"),
                    Subcategory = "subcategory-1",
                    TransactionId = "transaction-id-1",
                    TransactionType = "expense",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                };
                _mockTransactionRepository.Setup(repository => repository.GetTransactionById("id123"))
                    .ReturnsAsync(expectedTransaction);

                var returnedTransaction = await service.GetTransactionById("id123");

                Assert.Equal(expectedTransaction, returnedTransaction);
            }
        }

        public class GetTransactionsAsync
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public GetTransactionsAsync()
            {
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public async Task GivenValidInputs_ThenCorrectTransactionsReturned()
            {
                var service = new TransactionHelperService(
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
                    .Setup(repository => repository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
                        It.IsAny<ITransactionSpecification>()))
                    .ReturnsAsync(() => expectedTransactionList);

                var response = await service.GetTransactionsAsync(new GetTransactionsQuery());
                Assert.Equal(expectedTransactionList, response);
            }

            [Fact]
            public async Task GivenQueryInputs_ThenRepositoryCalledWithCorrectSpecification()
            {
                var expectedTransactionType = TransactionType.Expense;

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockMapper.Object);

                ITransactionSpecification calledWithSpecification = null;
                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
                        It.IsAny<ITransactionSpecification>()))
                    .Callback((DateRange _, ITransactionSpecification transactionSpecification) =>
                    {
                        calledWithSpecification = transactionSpecification;
                    });

                await service.GetTransactionsAsync(new GetTransactionsQuery
                {
                    Type = expectedTransactionType
                });

                Assert.IsType<AndSpec>(calledWithSpecification);
                Assert.True(calledWithSpecification.IsSatisfied(new DynamoDbTransaction
                {
                    TransactionType = "expense"
                }));
                Assert.False(calledWithSpecification.IsSatisfied(new DynamoDbTransaction
                {
                    TransactionType = "invalid type"
                }));
            }
        }

        public class StoreTransaction
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly IMapper _stubMapper;

            public StoreTransaction()
            {
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _stubMapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(TransactionHelperService)))
                    .CreateMapper();
            }

            [Fact]
            public async Task
                GivenValidStoreTransactionDto_ThenCorrectTransactionShouldBeStored()
            {
                var inputDto = new StoreTransactionDto
                {
                    Amount = (decimal) 1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    Subcategory = "subcategory-1",
                    TransactionType = "transaction-type-1",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                    Note = "this is a note123"
                };

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _stubMapper);

                await service.StoreTransaction(inputDto);

                Guid guid;
                _mockTransactionRepository.Verify(repository =>
                    repository.StoreTransaction(It.Is<Transaction>(transaction =>
                        transaction.TransactionTimestamp == inputDto.TransactionTimestamp &&
                        Guid.TryParse(transaction.TransactionId, out guid) &&
                        transaction.Amount == inputDto.Amount &&
                        transaction.Category == inputDto.Category &&
                        transaction.Subcategory == inputDto.Subcategory &&
                        transaction.TransactionType == inputDto.TransactionType &&
                        transaction.PayerPayeeId == inputDto.PayerPayeeId &&
                        transaction.PayerPayeeName == inputDto.PayerPayeeName &&
                        transaction.Note == inputDto.Note))
                );
            }
        }

        public class PutTransaction
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly IMapper _stubMapper;

            public PutTransaction()
            {
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _stubMapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(TransactionHelperService)))
                    .CreateMapper();
            }

            [Fact]
            public async Task GivenPutTransactionDto_ThenCorrectTransactionShouldBeUpdated()
            {
                var expectedTransactionId = Guid.NewGuid().ToString();

                var expectedTransaction = new Transaction()
                {
                    TransactionId = expectedTransactionId,
                    Amount = (decimal) 1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    Subcategory = "subcategory-1",
                    TransactionType = "type",
                    PayerPayeeId = "id123",
                    PayerPayeeName = "name123",
                    Note = "this is a note123"
                };

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _stubMapper);

                await service.PutTransaction(expectedTransactionId, new PutTransactionDto
                {
                    Amount = (decimal) 1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    Subcategory = "subcategory-1",
                    TransactionType = "type",
                    PayerPayeeId = "id123",
                    PayerPayeeName = "name123",
                    Note = "this is a note123"
                });

                _mockTransactionRepository.Verify(repository =>
                    repository.PutTransaction(expectedTransaction));
            }
        }

        public class DeleteTransaction
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository;
            private readonly Mock<IMapper> _mockMapper;

            public DeleteTransaction()
            {
                _mockTransactionRepository = new Mock<ITransactionRepository>();
                _mockMapper = new Mock<IMapper>();
            }

            [Fact]
            public async Task
                GivenTransactionId_WhenDeleteTransactionInvoked_ThenRepositoryDeleteTransactionCalledWithCorrectArgument()
            {
                var expectedTransactionId = Guid.NewGuid().ToString();

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockMapper.Object);
                await service.DeleteTransaction(expectedTransactionId);

                _mockTransactionRepository.Verify(repository =>
                    repository.DeleteTransaction(expectedTransactionId));
            }
        }
    }
}