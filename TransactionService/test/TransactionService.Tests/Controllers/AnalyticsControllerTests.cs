using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Services;
using TransactionService.ViewModels;
using Xunit;

namespace TransactionService.Tests.Controllers
{
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
            GivenValidQueryParams_WhenGetCategoryBreakdownInvoked_ThenAnalyticsServiceCalledWithCorrectArguments()
        {
            const string expectedType = "expense";
            const int expectedCount = 20;
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;

            var controller = new AnalyticsController(_mockAnalyticsService.Object);
            await controller.GetCategoryBreakdown(expectedType, expectedCount, startDate, endDate);

            _mockAnalyticsService.Verify(service =>
                service.GetCategoryBreakdown(expectedType, expectedCount, startDate, endDate));
        }

        [Fact]
        public async Task GivenValidQueryParams_WhenGetCategoryBreakdownInvoked_ThenReturnsListOfAnalyticsCategories()
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
                .Setup(service => service.GetCategoryBreakdown(It.IsAny<string>(), It.IsAny<int?>(),
                    It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(() => expectedAnalyticsCategories);
            var controller = new AnalyticsController(_mockAnalyticsService.Object);
            var response = await controller.GetCategoryBreakdown("expense");

            var objectResult = Assert.IsType<OkObjectResult>(response);

            var actualAnalyticsCategories = objectResult.Value as List<AnalyticsCategory>;

            Assert.Equal(expectedAnalyticsCategories, actualAnalyticsCategories);
        }

        [Fact]
        public async Task GivenValidQueryParameters_WhenGetPeriodicAggregatesInvoked_Then200OkReturned()
        {
            var controller = new AnalyticsController(_mockAnalyticsService.Object);
            var response = await controller.GetPeriodicAggregates("type", "period", 1);

            var objectResult = Assert.IsType<OkResult>(response);
        }
    }
}