using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Domain;
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
        public async Task GivenValidRequest_WhenGetInvoked_ThenCategoriesServiceGetAllCategoriesIsInvoked()
        {
            var controller = new CategoriesController(_mockCategoriesService.Object);
            await controller.Get();
            _mockCategoriesService.Verify(service => service.GetAllCategories());
        }

        [Fact]
        public async Task
            GivenCategoriesServiceReturnsObject_WhenGetInvoked_ThenReturns200OKWithCorrectListOfCategories()
        {
            var expectedCategoryList = new List<string>
            {
                "category1",
                "category2"
            };
            _mockCategoriesService.Setup(service => service.GetAllCategories()).ReturnsAsync(expectedCategoryList);

            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.Get();
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