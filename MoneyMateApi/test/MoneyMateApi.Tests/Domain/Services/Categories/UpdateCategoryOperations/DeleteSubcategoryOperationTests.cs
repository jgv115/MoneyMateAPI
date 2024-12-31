using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Categories.Exceptions;
using MoneyMateApi.Domain.Services.Categories.UpdateCategoryOperations;
using MoneyMateApi.Domain.Services.Transactions;
using MoneyMateApi.Repositories;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.Categories.UpdateCategoryOperations;

public class DeleteSubcategoryOperationTests
{
    private readonly Mock<ICategoriesRepository> _categoriesRepositoryMock = new();
    private readonly Mock<ITransactionHelperService> _transactionServiceMock = new();

    [Fact]
    public async Task GivenNonExistentCategory_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string categoryName = "categoryName";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object,
            _transactionServiceMock.Object, categoryName, 0);

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync((Category) null);

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenSubcategoryDoesNotExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string categoryName = "categoryName";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object,
            _transactionServiceMock.Object, categoryName, 2);

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync(new Category()
            {
                Subcategories = new List<string>() {"hello1", "hello2"}
            });

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenTransactionsWithSubcategoryStillExist_ThenUpdateCategoryOperationExceptionThrown()
    {
        const string categoryName = "categoryName";
        const string subcategoryName = "subcategory123";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object,
            _transactionServiceMock.Object, categoryName, 2);

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync(new Category()
            {
                Subcategories = new List<string>() {"hello1", "hello2", subcategoryName}
            });

        _transactionServiceMock.Setup(service => service.GetTransactionsAsync(new GetTransactionsQuery
        {
            Categories = new List<string> {categoryName},
            Subcategories = new List<string> {subcategoryName}
        })).ReturnsAsync(() => new List<Transaction>
        {
            new()
            {
                TransactionId = "id123",
                Amount = 123,
                Subcategory = "subcategory"
            },
            new()
            {
                TransactionId = "id1234",
                Amount = 123,
                Subcategory = "subcategory"
            }
        });

        await Assert.ThrowsAsync<UpdateCategoryOperationException>(() => operation.ExecuteOperation());
    }

    [Fact]
    public async Task GivenNoTransactionsWithSubcategoryExist_ThenRepositoryCalledWithCorrectArguments()
    {
        const string categoryName = "categoryName";
        const string subcategoryName = "subcategory name";
        var operation = new DeleteSubcategoryOperation(_categoriesRepositoryMock.Object,
            _transactionServiceMock.Object, categoryName, 2);

        _categoriesRepositoryMock.Setup(repository => repository.GetCategory(categoryName))
            .ReturnsAsync(new Category()
            {
                CategoryName = categoryName,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string>() {"hello1", "hello2", subcategoryName}
            });

        _transactionServiceMock.Setup(service => service.GetTransactionsAsync(new GetTransactionsQuery
        {
            Categories = new List<string> {categoryName},
            Subcategories = new List<string> {subcategoryName}
        })).ReturnsAsync(() => new List<Transaction>());

        await operation.ExecuteOperation();

        _categoriesRepositoryMock.Verify(repository => repository.DeleteSubcategory(categoryName, subcategoryName));
    }
}