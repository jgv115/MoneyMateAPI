using System;
using Microsoft.AspNetCore.JsonPatch.Operations;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public interface IUpdateCategoryOperationFactory
    {
        public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
            Operation<CategoryDto> jsonPatchOperation);
    }

    class UpdateCategoryOperationFactory : IUpdateCategoryOperationFactory
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly CurrentUserContext _userContext;

        public UpdateCategoryOperationFactory(ICategoriesRepository categoriesRepository,
            CurrentUserContext userContext)
        {
            _categoriesRepository = categoriesRepository;
            _userContext = userContext;
        }

        public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
            Operation<CategoryDto> jsonPatchOperation)
        {
            if (jsonPatchOperation.op == "add" && jsonPatchOperation.path == "/subcategories/-")
            
                return new AddSubcategoryOperation(jsonPatchOperation, _categoriesRepository, existingCategoryName,
                    _userContext);
            

            throw new Exception();
        }
    }
}