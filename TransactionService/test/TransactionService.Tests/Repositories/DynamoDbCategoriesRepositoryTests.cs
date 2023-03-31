using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories.DynamoDb;
using TransactionService.Repositories.DynamoDb.Models;
using TransactionService.Repositories.Exceptions;
using Xunit;

namespace TransactionService.Tests.Repositories;

public class DynamoDbCategoriesRepositoryTests
{
    private readonly Mock<IDynamoDBContext> _dynamoDbContextMock = new();
    private readonly IMapper _stubMapper;
    private const string UserId = "test-user-123";
    private const string TableName = "table-name123";

    private readonly DynamoDbRepositoryConfig _stubConfig = new()
    {
        TableName = TableName
    };

    private readonly CurrentUserContext _userContext = new()
    {
        UserId = UserId
    };

    public DynamoDbCategoriesRepositoryTests()
    {
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(DynamoDbCategoriesRepository)));
        _stubMapper = new Mapper(mapperConfig);
    }

    [Fact]
    public async Task GivenCategoryFound_WhenGetCategoryInvoked_ThenCorrectCategoryReturned()
    {
        const string categoryName = "category-name-123";

        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        _dynamoDbContextMock.Setup(context => context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = $"{UserId}#Categories",
                TransactionType = TransactionType.Expense
            });

        var returnedCategory = await repository.GetCategory(categoryName);

        Assert.Equal(new Category()
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense
        }, returnedCategory);
    }

    [Fact]
    public async Task GivenCategoryNotFound_WhenGetCategoryInvoked_ThenNullReturned()
    {
        const string categoryName = "category-name-123";

        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        _dynamoDbContextMock.Setup(context => context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.IsAny<DynamoDBOperationConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DynamoDbCategory) null);

        var returnedCategory = await repository.GetCategory(categoryName);

        Assert.Null(returnedCategory);
    }


    [Fact]
    public async Task
        GivenCategoriesReturnedFromDynamoDb_WhenGetAllCategoriesInvoked_ThenCorrectUserCategoryListReturned()
    {
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        var mockAsyncSearch = new Mock<AsyncSearch<DynamoDbCategory>>();

        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DynamoDbCategory>
            {
                new()
                {
                    CategoryName = "categoryName1",
                    UserId = $"{UserId}#Categories",
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"test1", "test2"}
                },
                new()
                {
                    CategoryName = "categoryName2",
                    UserId = $"{UserId}#Categories",
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"test3", "test4"}
                }
            });

        _dynamoDbContextMock.Setup(context =>
                context.QueryAsync<DynamoDbCategory>($"{UserId}#Categories",
                    It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName)))
            .Returns(mockAsyncSearch.Object);

        var returnedCategories = await repository.GetAllCategories();

        Assert.Equal(new List<Category>
        {
            new()
            {
                CategoryName = "categoryName1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", "test2"}
            },
            new()
            {
                CategoryName = "categoryName2",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test3", "test4"}
            }
        }, returnedCategories);
    }

    [Fact]
    public async Task
        GivenInputCategoryType_WhenGetAllCategoriesForTransactionTypeInvoked_ThenCorrectQueryFilterIsUsed()
    {
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        var mockAsyncSearch = new Mock<AsyncSearch<DynamoDbCategory>>();

        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DynamoDbCategory>());

        DynamoDBOperationConfig operationConfig = new();
        _dynamoDbContextMock.Setup(context =>
                context.QueryAsync<DynamoDbCategory>($"{UserId}#Categories",
                    It.IsAny<DynamoDBOperationConfig>()))
            .Callback((object _, DynamoDBOperationConfig config) => operationConfig = config)
            .Returns(mockAsyncSearch.Object);

        await repository.GetAllCategoriesForTransactionType(TransactionType.Expense);

        Assert.Equal(TableName, operationConfig.OverrideTableName);
        Assert.Collection(operationConfig.QueryFilter,
            condition =>
            {
                Assert.Equal("TransactionType", condition.PropertyName);
                Assert.Equal(ScanOperator.Equal, condition.Operator);
                Assert.Single(condition.Values);
                Assert.Equal(TransactionType.Expense, condition.Values[0]);
            }
        );
    }

    [Fact]
    public async Task
        GivenInputCategoryType_WhenGetAllCategoriesForTransactionTypeInvoked_ThenCorrectCategoriesReturned()
    {
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        var mockAsyncSearch = new Mock<AsyncSearch<DynamoDbCategory>>();

        mockAsyncSearch.Setup(search => search.GetRemainingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DynamoDbCategory>
            {
                new()
                {
                    CategoryName = "categoryName1",
                    UserId = $"{UserId}#Categories",
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"test1", "test2"}
                },
                new()
                {
                    CategoryName = "categoryName2",
                    UserId = $"{UserId}#Categories",
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"test3", "test4"}
                }
            });

        _dynamoDbContextMock.Setup(context =>
                context.QueryAsync<DynamoDbCategory>($"{UserId}#Categories",
                    It.IsAny<DynamoDBOperationConfig>()))
            .Returns(mockAsyncSearch.Object);

        var returnedCategories = await repository.GetAllCategoriesForTransactionType(TransactionType.Expense);

        Assert.Equal(new List<Category>
        {
            new()
            {
                CategoryName = "categoryName1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", "test2"}
            },
            new()
            {
                CategoryName = "categoryName2",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test3", "test4"}
            }
        }, returnedCategories);
    }

    [Fact]
    public async Task GivenCategoryAlreadyExists_WhenCreateCategoryInvoked_ThenRepositoryItemExistsExceptionThrown()
    {
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        const string categoryName = "category-name-123";

        _dynamoDbContextMock.Setup(context => context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = $"{UserId}#Categories",
                TransactionType = TransactionType.Expense
            });

        var inputCategory = new Category()
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        };

        await Assert.ThrowsAsync<RepositoryItemExistsException>(() => repository.CreateCategory(inputCategory));
    }

    [Fact]
    public async Task GivenCategoryDoesNotExist_WhenCreateCategoryInvoked_ThenCategorySavedCorrectly()
    {
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        const string categoryName = "category-name-123";

        _dynamoDbContextMock.Setup(context => context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((DynamoDbCategory) null);

        var inputCategory = new Category()
        {
            CategoryName = categoryName,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        };

        await repository.CreateCategory(inputCategory);

        _dynamoDbContextMock.Verify(context => context.SaveAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = $"{UserId}#Categories",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", "test2"}
            }, It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GivenCategoryAndNewCategoryName_WhenUpdateCategoryNameInvoked_ThenCategoryDeletedAndRecreated()
    {
        var inputCategory = new Category()
        {
            CategoryName = "categoryName",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        };
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        await repository.UpdateCategoryName(inputCategory, "new categoryName");

        _dynamoDbContextMock.Verify(context => context.DeleteAsync<DynamoDbCategory>($"{UserId}#Categories",
            "categoryName",
            It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));

        _dynamoDbContextMock.Verify(context => context.SaveAsync(new DynamoDbCategory()
            {
                CategoryName = "new categoryName",
                UserId = $"{UserId}#Categories",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", "test2"}
            }, It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GivenCategoryName_WhenDeleteCategoryInvoked_ThenCategoryDeletedCorrectly()
    {
        const string categoryName = "test-category-123";
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        await repository.DeleteCategory(categoryName);

        _dynamoDbContextMock.Verify(context => context.DeleteAsync<DynamoDbCategory>($"{UserId}#Categories",
            categoryName,
            It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GivenCategoryName_WhenGetAllSubcategoriesInvoked_ThenSubcategoriesQueriedCorrectly()
    {
        const string categoryName = "test-category-123";
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        var expectedSubcategories = new List<string> {"hello1", "hello2"};
        _dynamoDbContextMock.Setup(context =>
            context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>())).ReturnsAsync(() => new DynamoDbCategory
        {
            Subcategories = expectedSubcategories
        });

        var returnedSubcategories = await repository.GetAllSubcategories(categoryName);

        Assert.Equal(expectedSubcategories, returnedSubcategories);
    }

    [Fact]
    public async Task GivenNewSubcategoryName_WhenAddSubcategoryInvoked_ThenCategorySavedCorrectly()
    {
        const string categoryName = "test-category-123";
        const string newSubcategoryName = "new-subcategory";
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        _dynamoDbContextMock.Setup(context =>
            context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>())).ReturnsAsync(() => new DynamoDbCategory()
        {
            CategoryName = categoryName,
            UserId = UserId,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        });

        await repository.AddSubcategory(categoryName, newSubcategoryName);

        _dynamoDbContextMock.Verify(context => context.SaveAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = UserId,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", "test2", newSubcategoryName}
            }, It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GivenNewSubcategoryName_WhenUpdateSubcategoryNameInvoked_ThenCategorySavedCorrectly()
    {
        const string categoryName = "test-category-123";
        const string existingSubcategoryName = "existing-subcategory";
        const string newSubcategoryName = "new-subcategory";
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        _dynamoDbContextMock.Setup(context =>
            context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>())).ReturnsAsync(() => new DynamoDbCategory()
        {
            CategoryName = categoryName,
            UserId = UserId,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", existingSubcategoryName}
        });

        await repository.UpdateSubcategoryName(categoryName, existingSubcategoryName, newSubcategoryName);

        _dynamoDbContextMock.Verify(context => context.SaveAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = UserId,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test1", newSubcategoryName}
            }, It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GivenSubcategoryName_WhenDeleteSubcategoryInvoked_ThenCategorySavedCorrectly()
    {
        const string categoryName = "test-category-123";
        const string subcategoryName = "new-subcategory";
        var repository =
            new DynamoDbCategoriesRepository(_stubConfig, _dynamoDbContextMock.Object, _userContext, _stubMapper);

        _dynamoDbContextMock.Setup(context =>
            context.LoadAsync<DynamoDbCategory>($"{UserId}#Categories", categoryName,
                It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
                It.IsAny<CancellationToken>())).ReturnsAsync(() => new DynamoDbCategory()
        {
            CategoryName = categoryName,
            UserId = UserId,
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {subcategoryName, "test2"}
        });

        await repository.DeleteSubcategory(categoryName, subcategoryName);

        _dynamoDbContextMock.Verify(context => context.SaveAsync(new DynamoDbCategory()
            {
                CategoryName = categoryName,
                UserId = UserId,
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"test2"}
            }, It.Is<DynamoDBOperationConfig>(config => config.OverrideTableName == TableName),
            It.IsAny<CancellationToken>()));
    }
}