using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Moq;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Dtos;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain.Services.Categories.UpdateCategoryOperations;

public class UpdateCategoryOperationFactoryTests
{
    private readonly Mock<ICategoriesRepository> _mockCategoriesRepository = new();
    private readonly Mock<ITransactionHelperService> _mockTransactionHelperService = new();
    public string CategoryName = "name123";

    [Theory]
    [ClassData(typeof(PatchDocumentTestData))]
    public void GivenJsonPatchOperationForUpdatingACategory_ThenCorrectOperationReturned(
        Operation<CategoryDto> patchOperation, Type updateCategoryOperationType)
    {
        var factory =
            new UpdateCategoryOperationFactory(_mockCategoriesRepository.Object, _mockTransactionHelperService.Object);

        var updateCategoryOperation = factory.GetUpdateCategoryOperation(CategoryName, patchOperation);

        Assert.NotNull(updateCategoryOperation);
        Assert.IsType(updateCategoryOperationType, updateCategoryOperation);
    }

    public class PatchDocumentTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new Operation<CategoryDto>
                {
                    op = "add",
                    path = "/subcategories/-",
                    value = "new subcategory",
                },
                typeof(AddSubcategoryOperation)
            };

            yield return new object[]
            {
                new Operation<CategoryDto>
                {
                    op = "remove",
                    path = "/subcategories/1"
                },
                typeof(DeleteSubcategoryOperation)
            };


            // yield return new object[]
            // {
            //     new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
            //     {
            //         new()
            //         {
            //             op = "remove",
            //             path = "/subcategories/1",
            //         }
            //     }, new DefaultContractResolver()),
            //     new Category
            //     {
            //         UserId = UserId,
            //         CategoryName = CategoryName,
            //         TransactionType = TransactionType.Expense,
            //         Subcategories = new List<string> {"test1"}
            //     }
            // };
            //
            // yield return new object[]
            // {
            //     new JsonPatchDocument<CategoryDto>(new List<Operation<CategoryDto>>
            //     {
            //         new()
            //         {
            //             op = "replace",
            //             path = "/subcategories/1",
            //             value = "replaced subcategory name1"
            //         }
            //     }, new DefaultContractResolver()),
            //     new Category
            //     {
            //         UserId = UserId,
            //         CategoryName = CategoryName,
            //         TransactionType = TransactionType.Expense,
            //         Subcategories = new List<string> {"test1", "replaced subcategory name1"}
            //     }
            // };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}