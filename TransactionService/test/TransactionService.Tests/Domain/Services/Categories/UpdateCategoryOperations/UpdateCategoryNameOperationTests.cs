using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Constants;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Domain.Services.Transactions;
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
    public async Task GivenCategoryExists_ThenUpdateCategoryNameCalledWithCorrectArguments()
    {
        var operation = new UpdateCategoryNameOperation(_mockCategoryRepository.Object,
            _mockTransactionService.Object, _mockTransactionRepository.Object, "categoryName",
            "newCategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync(new Category()
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                TransactionType = TransactionType.Expense
            });

        await operation.ExecuteOperation();

        _mockCategoryRepository.Verify(repository =>
            repository.UpdateCategoryName(new Category()
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                TransactionType = TransactionType.Expense
            }, "newCategory"));
    }
}