using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Categories.UpdateCategoryOperations;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories
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

            return returnedCategories.OrderBy(category => category.CategoryName).ToList();
        }

        public Task CreateCategory(CategoryDto categoryDto)
        {
            var newCategory = _mapper.Map<Category>(categoryDto);

            newCategory.UserId = _userContext.UserId;
            return _repository.CreateCategory(newCategory);
        }

        // TODO: check if there are some transactions that belong to this category
        public Task DeleteCategory(string categoryName)
        {
            return _repository.DeleteCategory(categoryName);
        }

        // TODO: enforce one operation at a time.
        // TODO: Can't delete if there are still transactions
        // TODO: if want to change name then have to change it for all transactions
        // TODO: can't change transactionType
        public async Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument)
        {
            foreach (var patchOperation in patchDocument.Operations)
            {
                var updateCategoryOperation =
                    _updateCategoryOperationFactory.GetUpdateCategoryOperation(categoryName, patchOperation);
                await updateCategoryOperation.ExecuteOperation();
            }
            // patchDocument.Operations.ForEach(operation =>
            // {
            //     if (operation.op == "replace" && operation.path == "/transactionType")
            //         throw new BadUpdateCategoryRequestException("Updating transaction type is not allowed");
            // });
        }
    }
}