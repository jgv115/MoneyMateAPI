using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch.Operations;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public class AddSubcategoryOperation : IUpdateCategoryOperation
    {
        private readonly Operation<CategoryDto> _operation;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly string _existingCategoryName;
        private readonly CurrentUserContext _userContext;

        public AddSubcategoryOperation(Operation<CategoryDto> operation, ICategoriesRepository categoriesRepository,
            string existingCategoryName,
            CurrentUserContext userContext)
        {
            _operation = operation;
            _categoriesRepository = categoriesRepository;
            _existingCategoryName = existingCategoryName;
            _userContext = userContext;
        }

        public async Task ExecuteOperation()
        {
            var newSubcategoryName = (string) _operation.value;

            if (string.IsNullOrWhiteSpace(newSubcategoryName))
                throw new UpdateCategoryOperationException(
                    $"Failed to execute AddSubcategoryOperation, subcategory name is empty");

            var category = await _categoriesRepository.GetCategory(_userContext.UserId, _existingCategoryName);

            if (category is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute AddSubcategoryOperation, category {_existingCategoryName} does not exist");

            if (category.Subcategories.Contains(newSubcategoryName))
                throw new UpdateCategoryOperationException(
                    $"Failed to execute AddSubcategoryOperation, subcategory {newSubcategoryName} already exists");


            category.Subcategories.Add(newSubcategoryName);
            await _categoriesRepository.UpdateCategory(category);
        }
    }
}