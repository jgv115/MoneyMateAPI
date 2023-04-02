using System;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Operations;
using TransactionService.Controllers.Categories.Dtos;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;

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
                return new UpdateSubcategoryNameOperation(_categoriesRepository, existingCategoryName, subcategoryIndex,
                    newSubcategoryName);
            }
            else if (jsonPatchOperation.op == "replace" && jsonPatchOperation.path == "/categoryName")
            {
                var newCategoryName = (string) jsonPatchOperation.value;
                return new UpdateCategoryNameOperation(_categoriesRepository, _transactionHelperService,
                    _transactionRepository, existingCategoryName, newCategoryName);
            }

            // patchDocument.Operations.ForEach(operation =>
            // {
            //     if (operation.op == "replace" && operation.path == "/transactionType")
            //         throw new BadUpdateCategoryRequestException("Updating transaction type is not allowed");
            // });

            throw new Exception();
        }
    }
}