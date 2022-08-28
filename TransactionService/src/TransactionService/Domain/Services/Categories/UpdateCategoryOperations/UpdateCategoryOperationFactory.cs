using System;
using System.Linq;
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
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionHelperService _transactionHelperService;

        public UpdateCategoryOperationFactory(ICategoriesRepository categoriesRepository,
            ITransactionHelperService transactionHelperService, ITransactionRepository transactionRepository)
        {
            _categoriesRepository = categoriesRepository;
            _transactionHelperService = transactionHelperService;
            _transactionRepository = transactionRepository;
        }

        public IUpdateCategoryOperation GetUpdateCategoryOperation(string existingCategoryName,
            Operation<CategoryDto> jsonPatchOperation)
        {
            if (jsonPatchOperation.op == "add" && jsonPatchOperation.path == "/subcategories/-")
                return new AddSubcategoryOperation(jsonPatchOperation, _categoriesRepository, existingCategoryName);
            else if (jsonPatchOperation.op == "remove" && jsonPatchOperation.path.StartsWith("/subcategories/"))
            {
                var subcategoryIndex = int.Parse(jsonPatchOperation.path.Split("/").Last());
                return new DeleteSubcategoryOperation(_categoriesRepository, _transactionHelperService,
                    existingCategoryName, subcategoryIndex);
            }
            else if (jsonPatchOperation.op == "replace" && jsonPatchOperation.path.StartsWith("/subcategories/"))
            {
                var subcategoryIndex = int.Parse(jsonPatchOperation.path.Split("/").Last());
                var newSubcategoryName = (string) jsonPatchOperation.value;
                return new UpdateSubcategoryNameOperation(_categoriesRepository, _transactionHelperService,
                    _transactionRepository, existingCategoryName, subcategoryIndex, newSubcategoryName);
            }

            throw new Exception();
        }
    }
}