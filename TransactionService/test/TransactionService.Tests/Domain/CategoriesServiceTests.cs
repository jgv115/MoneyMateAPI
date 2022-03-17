using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain;

public class CategoriesServiceTests
{
    private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
    private readonly Mock<ICategoriesRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;

    public CategoriesServiceTests()
    {
        _mockCurrentUserContext = new Mock<CurrentUserContext>();
        _mockRepository = new Mock<ICategoriesRepository>();
        _mockMapper = new Mock<IMapper>();
    }

    [Fact]
    public void GivenNullUserContext_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CategoriesService(null, _mockRepository.Object, _mockMapper.Object));
    }

    [Fact]
    public void GivenNullRepository_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new CategoriesService(_mockCurrentUserContext.Object, null, _mockMapper.Object));
    }

    [Fact]
    public void GivenNullMapper_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object, null));
    }

    [Fact]
    public async Task
        GivenValidUserContext_WhenGetAllCategoryNamesInvoked_ThenRepositoryGetAllCategoriesCalledWithCorrectArguments()
    {
        const string expectedUserId = "userId-123";
        _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);

        await service.GetAllCategoryNames();

        _mockRepository.Verify(repository => repository.GetAllCategories(expectedUserId));
    }

    [Fact]
    public async Task GivenResponseFromRepository_WhenGetAllCategoryNamesInvoked_ThenCorrectCategoryListIsReturned()
    {
        var expectedReturnedCategoryList = new List<Category>
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

        _mockRepository.Setup(repository => repository.GetAllCategories(It.IsAny<string>()))
            .ReturnsAsync(expectedReturnedCategoryList);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);
        var response = await service.GetAllCategoryNames();

        Assert.Equal(expectedReturnedCategoryList.Select(category => category.CategoryName), response);
    }

    [Fact]
    public void
        GivenValidCategoryAndUserContext_WhenGetSubcategoriesInvoked_ThenRepositoryGetSubcategoriesCalledWithCorrectArguments()
    {
        const string expectedUserId = "testUser123";
        const string expectedCategory = "Category1";
        _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);
        service.GetSubcategories(expectedCategory);

        _mockRepository.Verify(repository => repository.GetAllSubcategories(expectedUserId, expectedCategory));
    }

    [Fact]
    public void
        GivenValidUserContextAndNullTransactionType_WhenGetAllCategoriesInvoked_ThenRepositoryGetAllCategoriesCalledWithTheCorrectArguments()
    {
        const string expectedUserId = "userId-123";
        _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);

        service.GetAllCategories();

        _mockRepository.Verify(repository => repository.GetAllCategories(expectedUserId));
    }

    [Theory]
    [InlineData(TransactionType.Expense)]
    [InlineData(TransactionType.Income)]
    public void
        GivenValidUserContextAndTransactionType_WhenGetAllCategoriesInvoked_ThenRepositoryGetAllCategoriesForTransactionTypeCalledWithTheCorrectArguments(
            TransactionType
                transactionType)
    {
        const string expectedUserId = "userId-123";
        _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);

        service.GetAllCategories(transactionType);

        _mockRepository.Verify(repository =>
            repository.GetAllCategoriesForTransactionType(expectedUserId, transactionType));
    }

    [Fact]
    public async Task
        GivenValidResponseFromRepository_WhenGetAllCategoriesInvokedWithNullTransactionType_ThenShouldReturnTheCorrectList()
    {
        var expectedReturnedCategoryList = new List<Category>
        {
            new()
            {
                CategoryName = "category1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        _mockRepository.Setup(repository => repository.GetAllCategories(It.IsAny<string>()))
            .ReturnsAsync(expectedReturnedCategoryList);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);
        var response = await service.GetAllCategories();

        Assert.Equal(expectedReturnedCategoryList, response);
    }

    [Fact]
    public async Task
        GivenValidResponseFromRepository_WhenGetAllCategoriesInvokedWithTransactionType_ThenShouldReturnTheCorrectList()
    {
        var expectedReturnedCategoryList = new List<Category>
        {
            new()
            {
                CategoryName = "category1",
                TransactionType = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            },
            new()
            {
                CategoryName = "category2",
                TransactionType = TransactionType.Income,
                Subcategories = new List<string> {"subcategory3", "subcategory4"}
            }
        };

        _mockRepository.Setup(repository =>
                repository.GetAllCategoriesForTransactionType(It.IsAny<string>(), It.IsAny<TransactionType>()))
            .ReturnsAsync(expectedReturnedCategoryList);

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);
        var response = await service.GetAllCategories(TransactionType.Expense);

        Assert.Equal(expectedReturnedCategoryList, response);
    }

    // TODO: take away mock mapper?
    [Fact]
    public async Task
        GivenValidCreateCategory_WhenCreateCategoryInvoked_ThenMapperShouldBeCalledWithCorrectArguments()
    {
        var expectedCategoryDto = new CreateCategoryDto
        {
            CategoryName = "testname",
            TransactionType = TransactionType.Expense,
            Subcategories = new List<string> {"test1", "test2"}
        };

        _mockMapper.Setup(mapper => mapper.Map<Category>(It.IsAny<CreateCategoryDto>())).Returns(new Category());

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);

        await service.CreateCategory(expectedCategoryDto);

        _mockMapper.Verify(mapper => mapper.Map<Category>(expectedCategoryDto));
    }

    [Fact]
    public async Task
        GivenCreateCategoryDto_WhenCreateCategoryInvoked_ThenRepositoryCreateCategoryInvokedWithCorrectArgument()
    {
        const string expectedUserId = "userId-123";
        _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

        const string expectedCategoryName = "categoryName";
        var expectedTransactionType = TransactionType.Expense;
        var expectedSubcategories = new List<string> {"sub1", "sub2"};

        var inputDto = new CreateCategoryDto
        {
            CategoryName = expectedCategoryName,
            TransactionType = expectedTransactionType,
            Subcategories = expectedSubcategories
        };

        var expectedCategory = new Category
        {
            UserId = expectedUserId,
            CategoryName = expectedCategoryName,
            TransactionType = expectedTransactionType,
            Subcategories = expectedSubcategories,
        };

        _mockMapper.Setup(mapper => mapper.Map<Category>(It.IsAny<CreateCategoryDto>())).Returns(new Category
        {
            CategoryName = expectedCategoryName,
            Subcategories = expectedSubcategories
        });

        var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
            _mockMapper.Object);

        await service.CreateCategory(inputDto);

        _mockRepository.Verify(repository => repository.CreateCategory(expectedCategory));
    }
}