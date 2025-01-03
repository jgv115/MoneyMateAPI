using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Transactions;
using MoneyMateApi.Domain.Services.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Repositories;
using MoneyMateApi.Tests.Common;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services
{
    public class TransactionHelperServiceTests
    {
        public class Constructor
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly Mock<IMapper> _mockMapper = new();

            [Fact]
            public void GivenNullITransactionRepository_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(null!, _mockTagRepository.Object, _mockMapper.Object));
            }

            [Fact]
            public void GivenNullIMapper_ThenArgumentNulExceptionIsThrown()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new TransactionHelperService(_mockTransactionRepository.Object, _mockTagRepository.Object,
                        null!));
            }
        }

        public class GetTransactionById
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly Mock<IMapper> _mockMapper = new();

            [Fact]
            public async Task GivenTransactionId_ThenTransactionFromRepositoryReturned()
            {
                var service = new TransactionHelperService(_mockTransactionRepository.Object, _mockTagRepository.Object,
                    _mockMapper.Object);

                var tagId1 = Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee35");
                var tagId2 = Guid.Parse("c6eae8c1-2514-4e21-9841-785db172ee36");
                var transactionDomainModel = new Transaction
                {
                    Amount = (decimal)1.0,
                    Category = "category-1",
                    TransactionTimestamp = DateTime.Now.ToString("O"),
                    Subcategory = "subcategory-1",
                    TransactionId = "transaction-id-1",
                    TransactionType = "expense",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                    Note = "note",
                    TagIds = [tagId1, tagId2]
                };

                _mockTransactionRepository.Setup(repository => repository.GetTransactionById("id123"))
                    .ReturnsAsync(transactionDomainModel);
                _mockTagRepository.Setup(repository => repository.GetTags(new List<Guid> { tagId1, tagId2 }))
                    .ReturnsAsync(() => new Dictionary<Guid, Tag>
                    {
                        { tagId1, new Tag(tagId1, tagId1.ToString()) },
                        { tagId2, new Tag(tagId2, tagId2.ToString()) }
                    });

                var returnedTransaction = await service.GetTransactionById("id123");

                _mockTagRepository.VerifyAll();

                var expectedOutput = new TransactionOutputDto
                {
                    Amount = (decimal)1.0,
                    Category = "category-1",
                    TransactionTimestamp = transactionDomainModel.TransactionTimestamp,
                    Subcategory = "subcategory-1",
                    TransactionId = "transaction-id-1",
                    TransactionType = "expense",
                    PayerPayeeId = transactionDomainModel.PayerPayeeId,
                    PayerPayeeName = "name1",
                    Note = "note",
                    Tags = [new Tag(tagId1, tagId1.ToString()), new Tag(tagId2, tagId2.ToString())]
                };
                Assert.Equal(expectedOutput, returnedTransaction);
            }
        }

        public class GetTransactionsAsync
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly Mock<IMapper> _mockMapper = new();


            [Fact]
            public async Task GivenValidInputs_ThenCorrectTransactionsReturned()
            {
                var service = new TransactionHelperService(_mockTransactionRepository.Object, _mockTagRepository.Object,
                    _mockMapper.Object);

                var builder = new TransactionListBuilder();
                Guid tagId1 = Guid.NewGuid(), tagId2 = Guid.NewGuid(), tagId3 = Guid.NewGuid();
                var transactionListBuilder = builder
                    .WithTransactions(
                        1,
                        Guid.NewGuid().ToString(),
                        "name1",
                        1,
                        TransactionType.Expense,
                        "category-1",
                        "subcategory-1", "note",
                        [tagId1, tagId2])
                    .WithTransactions(
                        1,
                        Guid.NewGuid().ToString(),
                        "name1",
                        1,
                        TransactionType.Expense,
                        "category-1",
                        "subcategory-1", "note",
                        [tagId3]);
                var transactionDomainModels = transactionListBuilder.BuildDomainModels();

                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
                        It.IsAny<ITransactionSpecification>()))
                    .ReturnsAsync(() => transactionDomainModels);

                var tagLookup = new Dictionary<Guid, Tag>
                {
                    { tagId1, new Tag(tagId1, tagId1.ToString()) },
                    { tagId2, new Tag(tagId2, tagId2.ToString()) },
                    { tagId3, new Tag(tagId3, tagId3.ToString()) }
                };
                _mockTagRepository.Setup(repository => repository.GetTags(new List<Guid> { tagId1, tagId2, tagId3 }))
                    .ReturnsAsync(() => tagLookup);

                var response = await service.GetTransactionsAsync(new GetTransactionsQuery());

                var expectedOutput = transactionListBuilder.BuildOutputDtos();
                Assert.Equal(expectedOutput, response);
            }

            [Fact]
            public async Task GivenTransactionsWithTags_ThenTagsArePopulated()
            {
                var service = new TransactionHelperService(_mockTransactionRepository.Object, _mockTagRepository.Object,
                    _mockMapper.Object);

                Guid tagId1 = Guid.NewGuid(), tagId2 = Guid.NewGuid();

                var transactionsBuilder = new TransactionListBuilder().WithTransactions(1, tagIds: [tagId1, tagId2]);
                var expectedTransactionList = transactionsBuilder.BuildDomainModels();

                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
                        It.IsAny<ITransactionSpecification>()))
                    .ReturnsAsync(() => expectedTransactionList);

                var tagLookup = new Dictionary<Guid, Tag>
                {
                    { tagId1, new Tag(tagId1, tagId1.ToString()) },
                    { tagId2, new Tag(tagId2, tagId2.ToString()) },
                };
                _mockTagRepository.Setup(repository => repository.GetTags(new List<Guid> { tagId1, tagId2 }))
                    .ReturnsAsync(() => tagLookup);

                var response = await service.GetTransactionsAsync(new GetTransactionsQuery());

                _mockTransactionRepository.VerifyAll();
                _mockTagRepository.VerifyAll();

                var expectedOutput = transactionsBuilder.BuildOutputDtos();
                Assert.Equal(expectedOutput, response);
            }

            [Fact]
            public async Task GivenQueryInputs_ThenRepositoryCalledWithCorrectSpecification()
            {
                var expectedTransactionType = TransactionType.Expense;

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockTagRepository.Object, _mockMapper.Object);

                ITransactionSpecification calledWithSpecification = null;
                _mockTransactionRepository
                    .Setup(repository => repository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
                        It.IsAny<ITransactionSpecification>()))
                    .Callback((DateRange _, ITransactionSpecification transactionSpecification) =>
                    {
                        calledWithSpecification = transactionSpecification;
                    }).ReturnsAsync(() => new List<Transaction>());

                await service.GetTransactionsAsync(new GetTransactionsQuery
                {
                    Type = expectedTransactionType
                });

                Assert.IsType<AndSpec>(calledWithSpecification);
                Assert.True(calledWithSpecification.IsSatisfied(new Transaction()
                {
                    TransactionType = "expense"
                }));
                Assert.False(calledWithSpecification.IsSatisfied(new Transaction()
                {
                    TransactionType = "invalid type"
                }));
            }
        }

        public class StoreTransaction
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly IMapper _stubMapper;

            public StoreTransaction()
            {
                _stubMapper = new MapperConfiguration(cfg => cfg.AddMaps(typeof(TransactionHelperService)))
                    .CreateMapper();
            }

            [Fact]
            public async Task
                GivenValidStoreTransactionDto_ThenCorrectTransactionShouldBeStored()
            {
                var inputDto = new StoreTransactionDto
                {
                    Amount = (decimal)1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    Subcategory = "subcategory-1",
                    TransactionType = "transaction-type-1",
                    PayerPayeeId = Guid.NewGuid().ToString(),
                    PayerPayeeName = "name1",
                    Note = "this is a note123",
                    TagIds = [Guid.NewGuid(), Guid.NewGuid()]
                };

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockTagRepository.Object, _stubMapper);

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
                        transaction.Note == inputDto.Note &&
                        transaction.TagIds.SequenceEqual(inputDto.TagIds)))
                );
            }
        }

        public class PutTransaction
        {
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly IMapper _stubMapper;

            public PutTransaction()
            {
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
                    Amount = (decimal)1.0,
                    TransactionTimestamp = "2021-04-13T13:15:23.7002027Z",
                    Category = "category-1",
                    Subcategory = "subcategory-1",
                    TransactionType = "type",
                    PayerPayeeId = "id123",
                    PayerPayeeName = "name123",
                    Note = "this is a note123"
                };

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockTagRepository.Object, _stubMapper);

                await service.PutTransaction(expectedTransactionId, new PutTransactionDto
                {
                    Amount = (decimal)1.0,
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
            private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();
            private readonly Mock<ITagRepository> _mockTagRepository = new();
            private readonly Mock<IMapper> _mockMapper = new();

            [Fact]
            public async Task
                GivenTransactionId_WhenDeleteTransactionInvoked_ThenRepositoryDeleteTransactionCalledWithCorrectArgument()
            {
                var expectedTransactionId = Guid.NewGuid().ToString();

                var service = new TransactionHelperService(
                    _mockTransactionRepository.Object, _mockTagRepository.Object, _mockMapper.Object);
                await service.DeleteTransaction(expectedTransactionId);

                _mockTransactionRepository.Verify(repository =>
                    repository.DeleteTransaction(expectedTransactionId));
            }
        }
    }
}