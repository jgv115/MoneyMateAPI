using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Categories;
using Xunit;

namespace MoneyMateApi.Tests.Controllers.Categories;

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
        Assert.Throws<ArgumentNullException>(() => new CategoriesController(null!));
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
        GivenValidRequest_WhenGetInvokedWithNonNullTransactionType_ThenCategoriesServiceGetAllCategoriesIsInvokedWithCorrectTransactionType()
    {
        var transactionType = TransactionType.Expense;
        var controller = new CategoriesController(_mockCategoriesService.Object);
        await controller.Get(transactionType);
        _mockCategoriesService.Verify(service => service.GetAllCategories(transactionType));
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

        _mockCategoriesService.Setup(service => service.GetAllCategories(It.IsAny<TransactionType>()))
            .ReturnsAsync(expectedCategoryList);

        var controller = new CategoriesController(_mockCategoriesService.Object);
        var response = await controller.Get(TransactionType.Expense);
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
        GivenCategoryNameQueryInput_WhenGetSubcategoriesInvoked_ThenReturns200OKWithCorrectListOfSubcategories()
    {
        var expectedSubcategoryList = new List<string>
        {
            "subCategory1",
            "subCategory2"
        };

        _mockCategoriesService.Setup(service => service.GetSubcategories(It.IsAny<string>()))
            .ReturnsAsync(expectedSubcategoryList);

        var controller = new CategoriesController(_mockCategoriesService.Object);
        var response = await controller.GetSubcategories("testCategory");
        var objectResponse = Assert.IsType<OkObjectResult>(response);

        Assert.Equal(expectedSubcategoryList, objectResponse.Value);
    }

    [Fact]
    public async Task
        GivenValidCategoryDto_WhenPostInvoked_ThenCategoriesServiceCreateCategoryIsCalledWithCorrectDto()
    {
        var expectedDto = new CategoryDto
        {
            CategoryName = "categoryName",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        };
        var controller = new CategoriesController(_mockCategoriesService.Object);
        await controller.Post(expectedDto);

        _mockCategoriesService.Verify(service => service.CreateCategory(expectedDto));
    }

    [Fact]
    public async Task GivenValidCategoryDto_WhenPostInvoked_Then200OKIsReturned()
    {
        var controller = new CategoriesController(_mockCategoriesService.Object);
        var response = await controller.Post(new CategoryDto
        {
            CategoryName = "categoryName",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        });

        var objectResponse = Assert.IsType<OkResult>(response);
        Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
    }

    [Fact]
    public async Task GivenCategoryName_WhenDeleteInvoked_ThenCategoriesServiceCalled()
    {
        const string categoryName = "category-name123";
        var controller = new CategoriesController(_mockCategoriesService.Object);

        await controller.Delete(categoryName);

        _mockCategoriesService.Verify(service => service.DeleteCategory(categoryName));
    }

    [Fact]
    public async Task GivenCategoryName_WhenDeleteInvoked_Then200OkReturned()
    {
        const string categoryName = "category-name123";
        var controller = new CategoriesController(_mockCategoriesService.Object);

        var response = await controller.Delete(categoryName);

        var objectResponse = Assert.IsType<OkResult>(response);
        Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
    }

    [Fact]
    public async Task
        GivenValidCategoryDto_WhenPatchInvoked_ThenCategoriesServiceCalledWithCorrectDto()
    {
        const string categoryName = "test category";
        var patchDoc = new JsonPatchDocument<CategoryDto>();

        var controller = new CategoriesController(_mockCategoriesService.Object);
        await controller.Patch(categoryName, patchDoc);

        _mockCategoriesService.Verify(service => service.UpdateCategory(categoryName, patchDoc));
    }

    [Fact]
    public async Task GivenValidCategoryDto_WhenPatchInvoked_Then200OKIsReturned()
    {
        var controller = new CategoriesController(_mockCategoriesService.Object);
        var response = await controller.Patch("category Name", new JsonPatchDocument<CategoryDto>());

        var objectResponse = Assert.IsType<OkResult>(response);
        Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
    }
}