using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch.Operations;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Dtos;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public class DeleteSubcategoryOperation : IUpdateCategoryOperation
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly string _existingCategoryName;
        private readonly string _subcategoryName;

        public DeleteSubcategoryOperation(ICategoriesRepository categoriesRepository, string existingCategoryName,
            string subcategoryName)
        {
            _categoriesRepository = categoriesRepository;
            _existingCategoryName = existingCategoryName;
            _subcategoryName = subcategoryName;
        }

        public async Task ExecuteOperation()
        {
            var category = await _categoriesRepository.GetCategory(_existingCategoryName);

            if (category is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute DeleteSubcategoryOperation, category {_existingCategoryName} does not exist");

            if (!category.Subcategories.Contains(_subcategoryName))
                throw new UpdateCategoryOperationException(
                    $"Failed to execute DeleteSubcategoryOperation, subcategory {_subcategoryName} under category {_existingCategoryName} does not exist");

            await _categoriesRepository.DeleteSubcategory(_existingCategoryName, _subcategoryName);
        }
    }
}