using System;
using Microsoft.AspNetCore.JsonPatch.Operations;
using TransactionService.Dtos;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public interface IUpdateCategoryOperationFactory
    {
        public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
            Operation<CategoryDto> jsonPatchOperation);
    }

    public class UpdateCategoryOperationFactory : IUpdateCategoryOperationFactory
    {
        private readonly ICategoriesRepository _categoriesRepository;

        public UpdateCategoryOperationFactory(ICategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
            Operation<CategoryDto> jsonPatchOperation)
        {
            if (jsonPatchOperation.op == "add" && jsonPatchOperation.path == "/subcategories/-")

                return new AddSubcategoryOperation(jsonPatchOperation, _categoriesRepository, existingCategoryName);


            throw new Exception();
        }
    }
}