using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Domain;
using TransactionService.Models;
using Xunit;

namespace TransactionService.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly Mock<ICategoriesService> _mockCategoriesService;

        public CategoriesControllerTests()
        {
            _mockCategoriesService = new Mock<ICategoriesService>();
        }

        [Fact]
        public void GivenNullCategoriesService_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoriesController(null));
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetInvokedWithNullCategoryType_ThenCategoriesServiceGetAllCategoriesIsInvokedWithCorrectCategoryType()
        {
            var controller = new CategoriesController(_mockCategoriesService.Object);
            await controller.Get(null);
            _mockCategoriesService.Verify(service => service.GetAllCategories(null));
        }
        
        [Fact]
        public async Task GivenValidRequest_WhenGetInvokedWithNonNullCategoryType_ThenCategoriesServiceGetAllCategoriesIsInvokedWithCorrectCategoryType()
        {
            var categoryType = "expense";
            var controller = new CategoriesController(_mockCategoriesService.Object);
            await controller.Get(categoryType);
            _mockCategoriesService.Verify(service => service.GetAllCategories(categoryType));
        }

        [Fact]
        public async Task
            GivenCategoriesServiceReturnsObject_WhenGetInvoked_ThenReturns200OKWithCorrectListOfCategories()
        {
            var expectedCategoryList = new List<Category>
            {
                new()
                {
                    CategoryName = "category1",
                    SubCategories = new List<string> {"subcategory1", "subcategory2"}
                },
                new()
                {
                    CategoryName = "category2",
                    SubCategories = new List<string> {"subcategory3", "subcategory4"}
                }
            };

            _mockCategoriesService.Setup(service => service.GetAllCategories(It.IsAny<string>())).ReturnsAsync(expectedCategoryList);

            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.Get("test");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedCategoryList, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidCategoryNameInput_WhenGetSubCategoriesInvoked_ThenCategoriesServiceGetSubCategoriesCalledWithCorrectCategory()
        {
            var expectedCategory = "category1";
            var controller = new CategoriesController(_mockCategoriesService.Object);

            await controller.GetSubCategories(expectedCategory);

            _mockCategoriesService.Verify(service => service.GetSubCategories(expectedCategory));
        }

        [Fact]
        public async Task
            GivenCategoryNameQueryInput_WhenGetSubCategoriesInvoked_ThenReturns200OKWithCorrectListOfSubCategories()
        {
            var expectedSubCategoryList = new List<string>
            {
                "subCategory1",
                "subCategory2"
            };

            _mockCategoriesService.Setup(service => service.GetSubCategories(It.IsAny<string>()))
                .ReturnsAsync(expectedSubCategoryList);

            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.GetSubCategories("testCategory");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(expectedSubCategoryList, objectResponse.Value);
        }
    }
}