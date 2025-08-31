using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Categories.UpdateCategoryOperations;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories;

namespace MoneyMateApi.Domain.Categories
{
    public class CategoriesService : ICategoriesService
    {
        private readonly CurrentUserContext _userContext;
        private readonly ICategoriesRepository _repository;
        private readonly IMapper _mapper;
        private readonly IUpdateCategoryOperationFactory _updateCategoryOperationFactory;

        public CategoriesService(CurrentUserContext userContext, ICategoriesRepository repository, IMapper mapper,
            IUpdateCategoryOperationFactory updateCategoryOperationFactory)
        {
            _userContext = userContext;
            _repository = repository;
            _mapper = mapper;
            _updateCategoryOperationFactory = updateCategoryOperationFactory;
        }

        public async Task<IEnumerable<string>> GetAllCategoryNames()
        {
            var categoriesList = await GetAllCategories();
            return categoriesList.Select(category => category.CategoryName);
        }

        public Task<IEnumerable<string>> GetSubcategories(string category)
        {
            return _repository.GetAllSubcategories(category);
        }

        public async Task<IEnumerable<Category>> GetAllCategories(TransactionType? transactionType = null)
        {
            IEnumerable<Category> returnedCategories;
            if (transactionType.HasValue)
                returnedCategories =
                    await _repository.GetAllCategoriesForTransactionType(transactionType.Value);
            else
                returnedCategories = await _repository.GetAllCategories();

            return returnedCategories.OrderBy(category => category.CategoryName);
        }

        public Task CreateCategory(CategoryDto categoryDto)
        {
            var newCategory = _mapper.Map<Category>(categoryDto);

            return _repository.CreateCategory(newCategory);
        }

        // TODO: check if there are some transactions that belong to this category
        public Task DeleteCategory(string categoryName)
        {
            return _repository.DeleteCategory(categoryName);
        }

        public async Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument)
        {
            foreach (var patchOperation in patchDocument.Operations)
            {
                var updateCategoryOperation =
                    _updateCategoryOperationFactory.GetUpdateCategoryOperation(categoryName, patchOperation);
                await updateCategoryOperation.ExecuteOperation();
            }
        }
    }
}