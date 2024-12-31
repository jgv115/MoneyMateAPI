using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers;
using MoneyMateApi.Controllers.Analytics;
using MoneyMateApi.Controllers.Analytics.ViewModels;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Controllers.PayersPayees.Dtos;
using MoneyMateApi.Services;
using Xunit;

namespace MoneyMateApi.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IAnalyticsService> _mockAnalyticsService;

    public AnalyticsControllerTests()
    {
        _mockAnalyticsService = new Mock<IAnalyticsService>();
    }

    [Fact]
    public void GivenNullService_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(() => new AnalyticsController(null));
    }

    [Fact]
    public async Task
        GivenValidQueryParams_WhenGetCategoriesBreakdownInvoked_ThenAnalyticsServiceCalledWithCorrectArguments()
    {
        var expectedType = TransactionType.Expense;
        const int expectedCount = 20;
        var startDate = DateTime.MinValue;
        var endDate = DateTime.MaxValue;

        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        await controller.GetCategoriesBreakdown(new GetCategoriesBreakdownQuery
        {
            Type = expectedType,
            Count = expectedCount,
            Start = startDate,
            End = endDate,
        });

        _mockAnalyticsService.Verify(service =>
            service.GetCategoriesBreakdown(expectedType, expectedCount, startDate, endDate));
    }

    [Fact]
    public async Task GivenValidQueryParams_WhenGetCategoriesBreakdownInvoked_ThenReturnsListOfAnalyticsCategories()
    {
        var expectedAnalyticsCategories = new List<AnalyticsCategory>
        {
            new()
            {
                CategoryName = "name1",
                TotalAmount = 123
            },
            new()
            {
                CategoryName = "name2",
                TotalAmount = 1234
            },
            new()
            {
                CategoryName = "name3",
                TotalAmount = 1232
            }
        };

        _mockAnalyticsService
            .Setup(service => service.GetCategoriesBreakdown(It.IsAny<TransactionType>(), It.IsAny<int?>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(() => expectedAnalyticsCategories);
        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetCategoriesBreakdown(new GetCategoriesBreakdownQuery
        {
            Type = TransactionType.Expense
        });

        var objectResult = Assert.IsType<OkObjectResult>(response);

        var actualAnalyticsCategories = objectResult.Value as List<AnalyticsCategory>;

        Assert.Equal(expectedAnalyticsCategories, actualAnalyticsCategories);
    }

    [Fact]
    public async Task
        GivenValidQueryParamsWithStartAndEnd_WhenGetSubcategoriesBreakdownInvoked_ThenReturnsListOfAnalyticsSubcategories()
    {
        const string expectedCategoryName = "category123";
        const int expectedCount = 20;
        var startDate = DateTime.MinValue;
        var endDate = DateTime.MaxValue;

        var expectedAnalyticsSubcategories = new List<AnalyticsSubcategory>
        {
            new()
            {
                SubcategoryName = "hello",
                TotalAmount = 11234,
                BelongsToCategory = "hello123"
            },
            new()
            {
                SubcategoryName = "hello2",
                TotalAmount = 112234,
                BelongsToCategory = "hello1234"
            }
        };

        _mockAnalyticsService.Setup(service =>
                service.GetSubcategoriesBreakdown(expectedCategoryName, expectedCount, startDate, endDate))
            .ReturnsAsync(() => expectedAnalyticsSubcategories);

        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetSubcategoriesBreakdown(new GetSubcategoriesBreakdownQuery
        {
            Category = expectedCategoryName,
            Count = expectedCount,
            End = endDate,
            Start = startDate
        });

        var objectResult = Assert.IsType<OkObjectResult>(response);

        var actualAnalyticsSubcategories = objectResult.Value as List<AnalyticsSubcategory>;

        Assert.Equal(expectedAnalyticsSubcategories, actualAnalyticsSubcategories);
    }

    [Fact]
    public async Task
        GivenValidQueryParamsWithoutStartAndEnd_WhenGetSubcategoriesBreakdownInvoked_ThenReturnsListOfAnalyticsSubcategories()
    {
        const string expectedCategoryName = "category123";
        const int expectedCount = 20;

        var expectedAnalyticsSubcategories = new List<AnalyticsSubcategory>
        {
            new()
            {
                SubcategoryName = "hello",
                TotalAmount = 11234,
                BelongsToCategory = "hello123"
            },
            new()
            {
                SubcategoryName = "hello2",
                TotalAmount = 112234,
                BelongsToCategory = "hello1234"
            }
        };

        _mockAnalyticsService.Setup(service =>
                service.GetSubcategoriesBreakdown(expectedCategoryName, expectedCount, DateTime.MinValue,
                    DateTime.MaxValue))
            .ReturnsAsync(() => expectedAnalyticsSubcategories);

        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetSubcategoriesBreakdown(new GetSubcategoriesBreakdownQuery
        {
            Category = expectedCategoryName,
            Count = expectedCount
        });

        var objectResult = Assert.IsType<OkObjectResult>(response);

        var actualAnalyticsSubcategories = objectResult.Value as List<AnalyticsSubcategory>;

        Assert.Equal(expectedAnalyticsSubcategories, actualAnalyticsSubcategories);
    }

    [Fact]
    public async Task
        GivenValidQueryParamsWithStartAndEnd_WhenGetPayerPayeeBreakdownInvoked_ThenReturnsListOfAnalyticsPayerPayees()
    {
        var expectedTransactionType = TransactionType.Expense;
        var startDate = DateTime.MinValue;
        var endDate = DateTime.MaxValue;

        var expectedAnalyticsPayerPayees = new List<AnalyticsPayerPayee>
        {
            new()
            {
                PayerPayeeName = "name",
                PayerPayeeId = Guid.NewGuid().ToString(),
                TotalAmount = 11234,
            }
        };

        _mockAnalyticsService.Setup(service =>
                service.GetPayerPayeeBreakdown(expectedTransactionType, startDate, endDate))
            .ReturnsAsync(() => expectedAnalyticsPayerPayees);

        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetPayerPayeesBreakdown(new GetPayerPayeesBreakdownQuery()
        {
            Type = expectedTransactionType,
            End = endDate,
            Start = startDate
        });

        var objectResult = Assert.IsType<OkObjectResult>(response);

        var actualAnalyticsPayerPayees = objectResult.Value as List<AnalyticsPayerPayee>;

        Assert.Equal(expectedAnalyticsPayerPayees, actualAnalyticsPayerPayees);
    }

    [Fact]
    public async Task
        GivenValidQueryParamsWithoutStartAndEnd_WhenGetPayerPayeesBreakdownInvoked_ThenReturnsListOfAnalyticsPayerPayees()
    {
        var expectedTransactionType = TransactionType.Expense;

        var expectedAnalyticsPayerPayees = new List<AnalyticsPayerPayee>
        {
            new()
            {
                PayerPayeeName = "name",
                PayerPayeeId = Guid.NewGuid().ToString(),
                TotalAmount = 11234,
            }
        };

        _mockAnalyticsService.Setup(service =>
                service.GetPayerPayeeBreakdown(expectedTransactionType, DateTime.MinValue,
                    DateTime.MaxValue))
            .ReturnsAsync(() => expectedAnalyticsPayerPayees);

        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetPayerPayeesBreakdown(new GetPayerPayeesBreakdownQuery()
        {
            Type = expectedTransactionType
        });

        var objectResult = Assert.IsType<OkObjectResult>(response);

        var actualAnalyticsPayerPayees = objectResult.Value as List<AnalyticsPayerPayee>;

        Assert.Equal(expectedAnalyticsPayerPayees, actualAnalyticsPayerPayees);
    }

    [Fact]
    public async Task GivenValidQueryParameters_WhenGetPeriodicAggregatesInvoked_Then200OkReturned()
    {
        var controller = new AnalyticsController(_mockAnalyticsService.Object);
        var response = await controller.GetPeriodicAggregates("type", "period", 1);

        var objectResult = Assert.IsType<OkResult>(response);
    }
}