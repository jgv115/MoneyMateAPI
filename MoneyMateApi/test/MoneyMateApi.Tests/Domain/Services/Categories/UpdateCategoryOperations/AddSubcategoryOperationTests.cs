using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Categories.Exceptions;
using MoneyMateApi.Domain.Services.Categories.UpdateCategoryOperations;
using MoneyMateApi.Repositories;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.Categories.UpdateCategoryOperations;

public class AddSubcategoryOperationTests
{
    private readonly Mock<ICategoriesRepository> _mockCategoriesRepository = new();

    [Fact]
    public async Task GivenValidSubcategory_ThenUpdateCategoryCalledWithCorrectCategoryModel()
    {
        const string existingCategoryName = "category123";
        var operation = new Operation<CategoryDto>
        {
            op = "add",
            path = "/subcategories/-",
            value = "new subcategory"
        };
        var handler = new AddSubcategoryOperation(operation, _mockCategoriesRepository.Object, existingCategoryName);

        _mockCategoriesRepository
            .Setup(repository => repository.GetCategory(existingCategoryName)).ReturnsAsync(
                new Category()
                {
                    CategoryName = existingCategoryName,
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"subcategory1", "subcategory2"}
                });

        await handler.ExecuteOperation();

        _mockCategoriesRepository.Verify(repository =>
            repository.AddSubcategory(existingCategoryName, "new subcategory"));
    }

    [Fact]
    public async Task GivenCategoryDoesNotExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string existingCategoryName = "non existant category";
        var operation = new Operation<CategoryDto>
        {
            op = "add",
            path = "/subcategories/-",
            value = "new subcategory"
        };

        var handler = new AddSubcategoryOperation(operation, _mockCategoriesRepository.Object, existingCategoryName);

        _mockCategoriesRepository
            .Setup(repository => repository.GetCategory(existingCategoryName))
            .ReturnsAsync((Category) null);

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => handler.ExecuteOperation());
    }

    [Fact]
    public async Task GivenSubcategoryAlreadyExists_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string existingCategoryName = "category123";
        const string subcategoryName = "new subcategory";
        var operation = new Operation<CategoryDto>
        {
            op = "add",
            path = "/subcategories/-",
            value = subcategoryName
        };
        var handler = new AddSubcategoryOperation(operation, _mockCategoriesRepository.Object, existingCategoryName);

        _mockCategoriesRepository
            .Setup(repository => repository.GetCategory(existingCategoryName)).ReturnsAsync(
                new Category()
                {
                    CategoryName = existingCategoryName,
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {subcategoryName, "subcategory1", "subcategory2"}
                });

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => handler.ExecuteOperation());
    }

    [Fact]
    public async Task GivenBlankSubcategoryName_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string existingCategoryName = "category123";
        var operation = new Operation<CategoryDto>
        {
            op = "add",
            path = "/subcategories/-",
            value = ""
        };
        var handler = new AddSubcategoryOperation(operation, _mockCategoriesRepository.Object, existingCategoryName);

        _mockCategoriesRepository
            .Setup(repository => repository.GetCategory(existingCategoryName)).ReturnsAsync(
                new Category()
                {
                    CategoryName = existingCategoryName,
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"subcategory1", "subcategory2"}
                });

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => handler.ExecuteOperation());
    }
}