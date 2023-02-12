using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Dtos;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain.Services.Categories.UpdateCategoryOperations;

public class UpdateCategoryNameOperationTests
{
    private readonly Mock<ICategoriesRepository> _mockCategoryRepository = new();
    private readonly Mock<ITransactionHelperService> _mockTransactionService = new();
    private readonly Mock<ITransactionRepository> _mockTransactionRepository = new();

    [Fact]
    public async Task GivenCategoryDoesNotExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        var operation = new UpdateCategoryNameOperation(_mockCategoryRepository.Object,
            _mockTransactionService.Object, _mockTransactionRepository.Object, "categoryName",
            "newCategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync((Category) null);

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenNoTransactionsExistWithCategory_ThenUpdateCategoryNameCalledWithCorrectArguments()
    {
        var operation = new UpdateCategoryNameOperation(_mockCategoryRepository.Object,
            _mockTransactionService.Object, _mockTransactionRepository.Object, "categoryName",
            "newCategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync(new Category
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                UserId = "us123",
                TransactionType = TransactionType.Expense
            });

        _mockTransactionService.Setup(service => service.GetTransactionsAsync(new GetTransactionsQuery
        {
            Categories = new List<string> {"categoryName"},
        })).ReturnsAsync(new List<Transaction>());

        await operation.ExecuteOperation();

        _mockCategoryRepository.Verify(repository =>
            repository.UpdateCategoryName(new Category
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                UserId = "us123",
                TransactionType = TransactionType.Expense
            }, "newCategory"));
    }

    [Fact]
    public async Task GivenTransactionsExistWithCategory_ThenTransactionsAreModifiedBeforeUpdatingCategoryName()
    {
        var operation = new UpdateCategoryNameOperation(_mockCategoryRepository.Object,
            _mockTransactionService.Object, _mockTransactionRepository.Object, "categoryName",
            "newCategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync(new Category
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                UserId = "us123",
                TransactionType = TransactionType.Expense
            });

        var transaction1 = new Transaction
        {
            UserId = "user123",
            Amount = 123,
            Category = "categoryName",
            TransactionTimestamp = DateTime.Now.ToString("O"),
            Subcategory = "subcategory",
            TransactionId = Guid.NewGuid().ToString(),
            TransactionType = "expense",
            PayerPayeeId = "id",
            PayerPayeeName = "name"
        };
        var transaction2 = new Transaction
        {
            UserId = "user123",
            Amount = 1234,
            Category = "categoryName",
            TransactionTimestamp = DateTime.Now.ToString("O"),
            Subcategory = "subcategory",
            TransactionId = Guid.NewGuid().ToString(),
            TransactionType = "expense",
            PayerPayeeId = "id",
            PayerPayeeName = "name"
        };

        _mockTransactionService.Setup(service => service.GetTransactionsAsync(new GetTransactionsQuery
        {
            Categories = new List<string> {"categoryName"},
        })).ReturnsAsync(new List<Transaction>
        {
            transaction1, transaction2
        });

        await operation.ExecuteOperation();

        transaction1.Category = "newCategory";
        transaction2.Category = "newCategory";
        _mockTransactionRepository.Verify(repository => repository.PutTransaction(transaction1), Times.Once);
        _mockTransactionRepository.Verify(repository => repository.PutTransaction(transaction2), Times.Once);


        _mockCategoryRepository.Verify(repository =>
            repository.UpdateCategoryName(new Category
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                UserId = "us123",
                TransactionType = TransactionType.Expense
            }, "newCategory"));
    }
}