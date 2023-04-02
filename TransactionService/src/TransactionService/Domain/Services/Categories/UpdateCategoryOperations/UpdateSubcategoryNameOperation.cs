using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public class UpdateSubcategoryNameOperation : IUpdateCategoryOperation
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly string _existingCategoryName;
        private readonly int _subcategoryIndex;
        private readonly string _newSubcategoryName;

        public UpdateSubcategoryNameOperation(
            ICategoriesRepository categoriesRepository,
            string existingCategoryName,
            int subcategoryIndex,
            string newSubcategoryName)
        {
            _categoriesRepository = categoriesRepository;
            _existingCategoryName = existingCategoryName;
            _subcategoryIndex = subcategoryIndex;
            _newSubcategoryName = newSubcategoryName;
        }


        public async Task ExecuteOperation()
        {
            var existingCategory = await _categoriesRepository.GetCategory(_existingCategoryName);

            if (existingCategory is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute UpdateSubcategoryNameOperation, category {_existingCategoryName} does not exist");

            var subcategoryName = existingCategory.Subcategories.ElementAtOrDefault(_subcategoryIndex);

            await _categoriesRepository.UpdateSubcategoryName(_existingCategoryName, subcategoryName,
                _newSubcategoryName);
        }
    }
}