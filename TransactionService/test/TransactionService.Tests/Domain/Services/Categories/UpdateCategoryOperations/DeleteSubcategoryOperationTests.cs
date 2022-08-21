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

public class DeleteSubcategoryOperationTests
{
    private readonly Mock<ICategoriesRepository> _categoriesRepositoryMock = new();

    [Fact]
    public async Task GivenNonExistentCategory_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string categoryName = "categoryName";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object, categoryName, "subcategory");

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync((Category) null);

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenSubcategoryDoesNotExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string categoryName = "categoryName";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object, categoryName, "subcategory");

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync(new Category
            {
                Subcategories = new List<string>() {"hello1", "hello2"}
            });

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenValidCategoryAndSubcategory_ThenRepositoryCalledWithCorrectArguments()
    {
        const string categoryName = "categoryName";
        const string subcategoryName = "subcategory name";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object, categoryName, subcategoryName);

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync(new Category
            {
                UserId = "user123",
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string>() {"hello1", "hello2", subcategoryName}
            });

        await operation.ExecuteOperation();

        _categoriesRepositoryMock.Verify(repository => repository.DeleteSubcategory(categoryName, subcategoryName));
    }
}