using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain.Services.Categories.UpdateCategoryOperations;

public class UpdateSubcategoryNameOperationTests
{
    private readonly Mock<ICategoriesRepository> _mockCategoryRepository = new();

    [Fact]
    public async Task GivenCategoryDoesNotExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        var operation = new UpdateSubcategoryNameOperation(_mockCategoryRepository.Object, "categoryName", 0,
            "newsubcategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync((Category) null);

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenCategoryDoesExist_ThenUpdateSubcategoryNameCalledWithCorrectArguments()
    {
        var operation = new UpdateSubcategoryNameOperation(_mockCategoryRepository.Object, "categoryName", 0,
            "newsubcategory");

        _mockCategoryRepository.Setup(repository => repository.GetCategory("categoryName"))
            .ReturnsAsync(new Category()
            {
                CategoryName = "categoryName",
                Subcategories = new List<string> {"subcategory", "test1"},
                TransactionType = TransactionType.Expense
            });

        await operation.ExecuteOperation();

        _mockCategoryRepository.Verify(repository =>
            repository.UpdateSubcategoryName("categoryName", "subcategory", "newsubcategory"));
    }
}