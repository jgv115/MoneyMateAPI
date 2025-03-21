using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using Newtonsoft.Json.Serialization;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Categories;
using MoneyMateApi.Domain.Services.Categories.UpdateCategoryOperations;
using MoneyMateApi.Middleware;
using MoneyMateApi.Profiles;
using MoneyMateApi.Repositories;
using Xunit;

namespace MoneyMateApi.Tests.Domain.Services.Categories;

public class FakeUpdateCategoryOperationFactory : IUpdateCategoryOperationFactory
{
    private readonly IUpdateCategoryOperation _mockUpdateCategoryOperation;

    public FakeUpdateCategoryOperationFactory(IUpdateCategoryOperation mockUpdateCategoryOperation)
    {
        _mockUpdateCategoryOperation = mockUpdateCategoryOperation;
    }

    public FakeUpdateCategoryOperationFactory()
    {
    }

    public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
        Operation<CategoryDto> jsonPatchOperation)
    {
        return _mockUpdateCategoryOperation;
    }
}

public class CategoriesServiceTests
{
    public class GetAllCategoryNames
    {
        private readonly Mock<ICategoriesRepository> _mockRepository = new();
        private readonly Mock<IMapper> _mockMapper = new();

        [Fact]
        public async Task
            GivenValidUserContext_ThenRepositoryGetAllCategoriesCalledWithCorrectArguments()
        {

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());

            await service.GetAllCategoryNames();

            _mockRepository.Verify(repository => repository.GetAllCategories());
        }

        [Fact]
        public async Task GivenResponseFromRepository_ThenCorrectCategoryListIsReturned()
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

            _mockRepository.Setup(repository => repository.GetAllCategories())
                .ReturnsAsync(expectedReturnedCategoryList);

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());
            var response = await service.GetAllCategoryNames();

            Assert.Equal(expectedReturnedCategoryList.Select(category => category.CategoryName), response);
        }
    }

    public class GetSubcategories
    {
        private readonly Mock<ICategoriesRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;

        public GetSubcategories()
        {
            _mockRepository = new Mock<ICategoriesRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public void
            GivenValidCategoryAndUserContext_ThenRepositoryGetSubcategoriesCalledWithCorrectArguments()
        {
            const string expectedCategory = "Category1";

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());
            service.GetSubcategories(expectedCategory);

            _mockRepository.Verify(repository => repository.GetAllSubcategories(expectedCategory));
        }
    }

    public class GetAllCategories
    {
        private readonly Mock<ICategoriesRepository> _mockRepository;
        private readonly Mock<IMapper> _mockMapper;

        public GetAllCategories()
        {
            _mockRepository = new Mock<ICategoriesRepository>();
            _mockMapper = new Mock<IMapper>();
        }


        [Fact]
        public async Task
            GivenValidUserContextAndNullTransactionType_ThenRepositoryGetAllCategoriesCalledWithTheCorrectArguments()
        {

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());

            await service.GetAllCategories();

            _mockRepository.Verify(repository => repository.GetAllCategories());
        }

        [Theory]
        [InlineData(TransactionType.Expense)]
        [InlineData(TransactionType.Income)]
        public async Task
            GivenValidUserContextAndTransactionType_ThenRepositoryGetAllCategoriesForTransactionTypeCalledWithTheCorrectArguments(
                TransactionType
                    transactionType)
        {

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());

            await service.GetAllCategories(transactionType);

            _mockRepository.Verify(repository =>
                repository.GetAllCategoriesForTransactionType(transactionType));
        }

        [Fact]
        public async Task
            GivenNullTransactionType_ThenShouldReturnTheCorrectList()
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

            _mockRepository.Setup(repository => repository.GetAllCategories())
                .ReturnsAsync(expectedReturnedCategoryList);

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());
            var response = await service.GetAllCategories();

            Assert.Equal(expectedReturnedCategoryList, response);
        }

        [Fact]
        public async Task GivenTransactionType_ThenShouldReturnTheCorrectList()
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
                    repository.GetAllCategoriesForTransactionType(It.IsAny<TransactionType>()))
                .ReturnsAsync(expectedReturnedCategoryList);

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());
            var response = await service.GetAllCategories(TransactionType.Expense);

            Assert.Equal(expectedReturnedCategoryList, response);
        }

        [Fact]
        public async Task GivenResponseFromRepository_ThenShouldReturnCategoriesInAlphabeticalOrder()
        {
            var returnedCategoryList = new List<Category>
            {
                new()
                {
                    CategoryName = "category2",
                    TransactionType = TransactionType.Income,
                    Subcategories = new List<string> {"subcategory3", "subcategory4"}
                },
                new()
                {
                    CategoryName = "category1",
                    TransactionType = TransactionType.Expense,
                    Subcategories = new List<string> {"subcategory1", "subcategory2"}
                }
            };

            _mockRepository.Setup(repository =>
                    repository.GetAllCategoriesForTransactionType(It.IsAny<TransactionType>()))
                .ReturnsAsync(returnedCategoryList);

            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());
            var response = await service.GetAllCategories(TransactionType.Expense);

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
            Assert.Equal(expectedReturnedCategoryList, response);
        }
    }

    public class CreateCategory
    {
        private readonly CurrentUserContext _mockCurrentUserContext;
        private readonly Mock<ICategoriesRepository> _mockRepository;
        private readonly IMapper _stubMapper;

        public CreateCategory()
        {
            _mockCurrentUserContext = new CurrentUserContext();
            _mockRepository = new Mock<ICategoriesRepository>();
            _stubMapper = new MapperConfiguration(cfg => { cfg.AddProfile<CategoryProfile>(); }).CreateMapper();
        }

        [Fact]
        public async Task
            GivenCategoryDto_ThenRepositoryCreateCategoryInvokedWithCorrectArgument()
        {

            const string expectedCategoryName = "categoryName";
            var expectedTransactionType = TransactionType.Expense;
            var expectedSubcategories = new List<string> {"sub1", "sub2"};

            var inputDto = new CategoryDto
            {
                CategoryName = expectedCategoryName,
                TransactionType = expectedTransactionType,
                Subcategories = expectedSubcategories
            };

            var expectedCategory = new Category()
            {
                CategoryName = expectedCategoryName,
                TransactionType = expectedTransactionType,
                Subcategories = expectedSubcategories,
            };

            var service = new CategoriesService(_mockCurrentUserContext, _mockRepository.Object,
                _stubMapper, new FakeUpdateCategoryOperationFactory());

            await service.CreateCategory(inputDto);

            _mockRepository.Verify(repository => repository.CreateCategory(expectedCategory));
        }
    }

    public class DeleteCategory
    {
        private readonly CurrentUserContext _mockCurrentUserContext;
        private readonly Mock<ICategoriesRepository> _mockRepository;

        private readonly Mock<IMapper> _mockMapper;

        private const string UserId = "userId-123";

        public DeleteCategory()
        {
            _mockCurrentUserContext = new CurrentUserContext
            {
                UserId = UserId
            };
            _mockRepository = new Mock<ICategoriesRepository>();
            _mockMapper = new Mock<IMapper>();
        }

        [Fact]
        public async Task GivenCategoryName_ThenRepositoryDeleteCategoryCalledWithCorrectArguments()
        {
            const string categoryName = "category Name 123";
            var service = new CategoriesService(_mockCurrentUserContext, _mockRepository.Object,
                _mockMapper.Object, new FakeUpdateCategoryOperationFactory());

            _mockRepository.Setup(repository => repository.DeleteCategory(It.IsAny<string>()));

            await service.DeleteCategory(categoryName);

            _mockRepository.Verify(repository => repository.DeleteCategory(categoryName));
        }
    }

    public class UpdateCategory
    {
        private readonly Mock<ICategoriesRepository> _mockRepository;

        private readonly IMapper _mapper;


        public UpdateCategory()
        {
            _mockRepository = new Mock<ICategoriesRepository>();

            _mapper = new MapperConfiguration(cfg => { cfg.AddProfile<CategoryProfile>(); }).CreateMapper();

        }

        [Fact]
        public async Task GivenJsonPatchDocuments_ThenCorrectUpdateCategoryOperationsAreExecuted()
        {
            var mockUpdateCategoryOperation = new Mock<IUpdateCategoryOperation>();
            var service = new CategoriesService(new CurrentUserContext(), _mockRepository.Object,
                _mapper, new FakeUpdateCategoryOperationFactory(mockUpdateCategoryOperation.Object));

            const string expectedCategoryName = "category123";
            var inputPatchDocument = new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
            {
                new()
                {
                    op = "add",
                    path = "/subcategories/-",
                    value = "test1"
                },
                new()
                {
                    op = "add",
                    path = "/subcategories/-",
                    value = "test2"
                }
            }, new DefaultContractResolver());

            await service.UpdateCategory(expectedCategoryName, inputPatchDocument);

            mockUpdateCategoryOperation.Verify(operation => operation.ExecuteOperation(),
                Times.Exactly(inputPatchDocument.Operations.Count));
        }

        // [Fact]
        // public async Task GivenCategoryNameThatDoesNotExist_ThenBadUpdateCategoryRequestExceptionThrown()
        // {
        //     _mockRepository.Setup(repository => repository.GetCategory("name"))
        //         .ReturnsAsync(() => null);
        //
        //     var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
        //         _mapper, new FakeUpdateCategoryOperationFactory());
        //
        //     await Assert.ThrowsAsync<BadUpdateCategoryRequestException>(() =>
        //         service.UpdateCategory("name", new JsonPatchDocument<CategoryDto>()));
        // }
        //
        // [Fact]
        // public async Task GivenAttemptToAddEmptySubcategory_ThenBadUpdateCategoryRequestExceptionThrown()
        // {
        //     const string categoryName = "test-category";
        //     var inputPatchDocument = new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
        //     {
        //         new()
        //         {
        //             op = "add",
        //             path = "/subcategories/-",
        //             value = ""
        //         }
        //     }, new DefaultContractResolver());
        //
        //     _mockRepository.Setup(repository => repository.GetCategory(categoryName))
        //         .ReturnsAsync(() => new Category());
        //
        //     var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
        //         _mapper, new FakeUpdateCategoryOperationFactory());
        //
        //     await Assert.ThrowsAsync<BadUpdateCategoryRequestException>(() =>
        //         service.UpdateCategory(categoryName, inputPatchDocument));
        // }
        //
        // [Fact]
        // public async Task GivenAttemptToChangeTransactionType_ThenBadUpdateCategoryRequestExceptionThrown()
        // {
        //     const string categoryName = "test-category";
        //
        //     var inputPatchDocument = new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
        //     {
        //         new()
        //         {
        //             op = "replace",
        //             path = "/transactionType",
        //             value = TransactionType.Expense
        //         }
        //     }, new DefaultContractResolver());
        //
        //     _mockRepository.Setup(repository => repository.GetCategory(categoryName))
        //         .ReturnsAsync(() => new Category());
        //
        //     var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
        //         _mapper, new FakeUpdateCategoryOperationFactory());
        //
        //     await Assert.ThrowsAsync<BadUpdateCategoryRequestException>(() =>
        //         service.UpdateCategory(categoryName, inputPatchDocument));
        // }
        //
        // [Theory]
        // [ClassData(typeof(PatchDocumentTestData))]
        // public async Task
        //     GivenCategoryNameAndPatchDocument_ThenRepositoryUpdateCategoryCalledWithCorrectModel(
        //         JsonPatchDocument<CategoryDto> patchDoc, Category expectedCategory)
        // {
        //     var testData = new PatchDocumentTestData();
        //
        //     _mockRepository.Setup(repository => repository.GetCategory(testData.CategoryName))
        //         .ReturnsAsync(() => testData.ExistingCategory);
        //
        //     var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object,
        //         _mapper, new FakeUpdateCategoryOperationFactory());
        //
        //     await service.UpdateCategory(testData.CategoryName, patchDoc);
        //
        //     _mockRepository.Verify(repository => repository.UpdateCategory(expectedCategory));
        // }
        //
        // public class PatchDocumentTestData : IEnumerable<object[]>
        // {
        //     public string CategoryName = "name123";
        //
        //     public Category ExistingCategory;
        //
        //     public PatchDocumentTestData()
        //     {
        //         ExistingCategory = new Category
        //         {
        //             UserId = UserId,
        //             CategoryName = CategoryName,
        //             TransactionType = TransactionType.Expense,
        //             Subcategories = new List<string> {"test1", "test2"}
        //         };
        //     }
        //
        //     public IEnumerator<object[]> GetEnumerator()
        //     {
        //         yield return new object[]
        //         {
        //             new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
        //             {
        //                 new()
        //                 {
        //                     op = "add",
        //                     path = "/subcategories/-",
        //                     value = "new subcategory",
        //                 }
        //             }, new DefaultContractResolver()),
        //             new Category
        //             {
        //                 UserId = UserId,
        //                 CategoryName = CategoryName,
        //                 TransactionType = TransactionType.Expense,
        //                 Subcategories = new List<string> {"test1", "test2", "new subcategory"}
        //             }
        //         };
        //
        //         yield return new object[]
        //         {
        //             new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
        //             {
        //                 new()
        //                 {
        //                     op = "remove",
        //                     path = "/subcategories/1",
        //                 }
        //             }, new DefaultContractResolver()),
        //             new Category
        //             {
        //                 UserId = UserId,
        //                 CategoryName = CategoryName,
        //                 TransactionType = TransactionType.Expense,
        //                 Subcategories = new List<string> {"test1"}
        //             }
        //         };
        //
        //         yield return new object[]
        //         {
        //             new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
        //             {
        //                 new()
        //                 {
        //                     op = "replace",
        //                     path = "/subcategories/1",
        //                     value = "replaced subcategory name1"
        //                 }
        //             }, new DefaultContractResolver()),
        //             new Category
        //             {
        //                 UserId = UserId,
        //                 CategoryName = CategoryName,
        //                 TransactionType = TransactionType.Expense,
        //                 Subcategories = new List<string> {"test1", "replaced subcategory name1"}
        //             }
        //         };
        //     }
        //
        //     IEnumerator IEnumerable.GetEnumerator()
        //     {
        //         return GetEnumerator();
        //     }
        // }
    }
}