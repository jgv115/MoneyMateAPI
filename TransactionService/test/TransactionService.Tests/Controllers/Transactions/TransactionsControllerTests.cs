using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Constants;
using TransactionService.Controllers.Transactions;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions;
using Xunit;

namespace TransactionService.Tests.Controllers.Transactions
{
    public class TransactionsControllerTests
    {
        private readonly Mock<ITransactionHelperService> _mockTransactionHelperService;

        public TransactionsControllerTests()
        {
            _mockTransactionHelperService = new Mock<ITransactionHelperService>();
        }

        [Fact]
        public async Task GivenValidTransactinoId_WhenGetByIdIsInvoked_ThenReturns200Ok()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);

            _mockTransactionHelperService.Setup(service => service.GetTransactionById("id123"))
                .ReturnsAsync(new Transaction());

            var response = await controller.GetById("id123");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }

        [Fact]
        public async Task GivenValidTransactinoId_WhenGetByIdIsInvoked_ThenCorrectTransactionReturned()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);

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
            _mockTransactionHelperService.Setup(service => service.GetTransactionById("id123"))
                .ReturnsAsync(expectedTransaction);

            var response = await controller.GetById("id123");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(expectedTransaction, objectResponse.Value as Transaction);
        }

        [Fact]
        public async Task GivenValidQueryParams_WhenGetIsInvoked_ThenReturns200Ok()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);

            var response = await controller.Get(new GetTransactionsQuery());
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }

        [Fact]
        public async Task GivenValidQueryParams_WhenGetIsInvoked_ThenReturnsAListOfTransactions()
        {
            var transaction1 = new Transaction
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

            var transaction2 = new Transaction
            {
                Amount = (decimal) 2.0,
                Category = "category-2",
                TransactionTimestamp = DateTime.Now.ToString("O"),
                Subcategory = "subcategory-2",
                TransactionId = "transaction-id-2",
                TransactionType = "expense",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
            };

            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2
            };

            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;
            var type = TransactionType.Expense;
            var inputQuery = new GetTransactionsQuery
            {
                Start = startDate,
                End = endDate,
                Type = type
            };

            _mockTransactionHelperService.Setup(service =>
                    service.GetTransactionsAsync(inputQuery))
                .ReturnsAsync(expectedTransactionList);

            var controller = new TransactionsController(_mockTransactionHelperService.Object);
            var response = await controller.Get(inputQuery);

            var objectResult = Assert.IsType<OkObjectResult>(response);

            var transactionList = objectResult.Value as List<Transaction>;
            Assert.NotNull(transactionList);
            Assert.Equal(expectedTransactionList, transactionList);
        }

        [Fact]
        public async Task GivenValidInputDto_WhenPostIsInvoked_Then200OkIsReturned()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);
            var response = await controller.Post(new StoreTransactionDto());

            var objectResponse = Assert.IsType<OkResult>(response);
            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }

        [Fact]
        public async Task GivenValidInputDto_WhenPutIsInvoked_Then200OkIsReturned()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);
            var response = await controller.Put("transaction-id-123", new PutTransactionDto());

            var objectResponse = Assert.IsType<OkResult>(response);
            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }

        [Fact]
        public async Task
            GivenValidInputTransactionId_WhenDeleteIsInvoked_ThenTransactionHelperServiceCalledWithCorrectParameters()
        {
            var expectedTransactionId = "test12354";
            var controller = new TransactionsController(_mockTransactionHelperService.Object);

            await controller.Delete(expectedTransactionId);

            _mockTransactionHelperService.Verify(service => service.DeleteTransaction(expectedTransactionId));
        }

        [Fact]
        public async Task GivenSuccessfulTransactionHelperServiceCall_WhenDeleteIsInvoked_Then204NoContentIsReturned()
        {
            var controller = new TransactionsController(_mockTransactionHelperService.Object);
            var response = await controller.Delete("test1234");

            var objectResponse = Assert.IsType<NoContentResult>(response);
            Assert.Equal(StatusCodes.Status204NoContent, objectResponse.StatusCode);
        }
    }
}