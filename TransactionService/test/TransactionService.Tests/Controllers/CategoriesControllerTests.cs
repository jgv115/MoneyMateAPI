using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Dtos;
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
        public async Task
            GivenValidRequest_WhenGetInvokedWithNullCategoryType_ThenCategoriesServiceGetAllCategoriesIsInvokedWithCorrectCategoryType()
        {
            var controller = new CategoriesController(_mockCategoriesService.Object);
            await controller.Get(null);
            _mockCategoriesService.Verify(service => service.GetAllCategories(null));
        }

        [Fact]
        public async Task
            GivenValidRequest_WhenGetInvokedWithNonNullCategoryType_ThenCategoriesServiceGetAllCategoriesIsInvokedWithCorrectCategoryType()
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
                    Subcategories = new List<string> {"subcategory1", "subcategory2"}
                },
                new()
                {
                    CategoryName = "category2",
                    Subcategories = new List<string> {"subcategory3", "subcategory4"}
                }
            };

            _mockCategoriesService.Setup(service => service.GetAllCategories(It.IsAny<string>()))
                .ReturnsAsync(expectedCategoryList);

            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.Get("test");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedCategoryList, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidCategoryNameInput_WhenGetSubcategoriesInvoked_ThenCategoriesServiceGetSubcategoriesCalledWithCorrectCategory()
        {
            var expectedCategory = "category1";
            var controller = new CategoriesController(_mockCategoriesService.Object);

            await controller.GetSubcategories(expectedCategory);

            _mockCategoriesService.Verify(service => service.GetSubcategories(expectedCategory));
        }

        [Fact]
        public async Task
            GivenCategoryNameQueryInput_WhenGetSubcategoriesInvoked_ThenReturns200OKWithCorrectListOfSubCategories()
        {
            var expectedSubCategoryList = new List<string>
            {
                "subCategory1",
                "subCategory2"
            };

            _mockCategoriesService.Setup(service => service.GetSubcategories(It.IsAny<string>()))
                .ReturnsAsync(expectedSubCategoryList);

            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.GetSubcategories("testCategory");
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(expectedSubCategoryList, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidCreateCategoryDto_WhenPostInvoked_ThenCategoriesServiceCreateCategoryIsCalledWithCorrectDto()
        {
            var expectedDto = new CreateCategoryDto
            {
                CategoryName = "categoryName",
                CategoryType = "categoryType",
                Subcategories = new List<string> {"test1", "test2"}
            };
            var controller = new CategoriesController(_mockCategoriesService.Object);
            await controller.Post(expectedDto);

            _mockCategoriesService.Verify(service => service.CreateCategory(expectedDto));
        }

        [Fact]
        public async Task GivenValidCreateCategoryDto_WhenPostInvoked_Then200OKIsReturned()
        {
            var controller = new CategoriesController(_mockCategoriesService.Object);
            var response = await controller.Post(new CreateCategoryDto
            {
                CategoryName = "categoryName",
                CategoryType = "categoryType",
                Subcategories = new List<string> {"test1", "test2"}
            });

            var objectResponse = Assert.IsType<OkResult>(response);
            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }
    }
}