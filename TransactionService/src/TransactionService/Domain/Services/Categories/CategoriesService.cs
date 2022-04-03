using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Categories.Exceptions;
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

        public CategoriesService(CurrentUserContext userContext, ICategoriesRepository repository, IMapper mapper)
        {
            _userContext = userContext;
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetAllCategoryNames()
        {
            var categoriesList = await GetAllCategories();
            return categoriesList.Select(category => category.CategoryName);
        }

        public Task<IEnumerable<string>> GetSubcategories(string category)
        {
            return _repository.GetAllSubcategories(_userContext.UserId, category);
        }

        public async Task<IEnumerable<Category>> GetAllCategories(TransactionType? transactionType = null)
        {
            IEnumerable<Category> returnedCategories;
            if (transactionType.HasValue)
                returnedCategories =
                    await _repository.GetAllCategoriesForTransactionType(_userContext.UserId, transactionType.Value);
            else
                returnedCategories = await _repository.GetAllCategories(_userContext.UserId);

            return returnedCategories.OrderBy(category => category.CategoryName).ToList();
        }

        public Task CreateCategory(CategoryDto categoryDto)
        {
            var newCategory = _mapper.Map<Category>(categoryDto);

            newCategory.UserId = _userContext.UserId;
            return _repository.CreateCategory(newCategory);
        }

        // TODO: enforce one operation at a time.
        // TODO: Can't delete if there are still transactions
        // TODO: if want to change name then have to change it for all transactions
        // TODO: can't change transactionType
        public async Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument)
        {
            patchDocument.Operations.ForEach(operation =>
            {
                if (operation.op == "add" && operation.path == "/subcategories/-")
                {
                    if (string.IsNullOrWhiteSpace(operation.value as string))
                        throw new BadUpdateCategoryRequestException("Subcategory name should not be empty");
                }

                if (operation.op == "replace" && operation.path == "/transactionType")
                    throw new BadUpdateCategoryRequestException("Updating transaction type is not allowed");
            });

            var existingCategory = await _repository.GetCategory(_userContext.UserId, categoryName);
            if (existingCategory == null)
                throw new BadUpdateCategoryRequestException($"Category {categoryName} does not exist");

            var existingCategoryDto = _mapper.Map<CategoryDto>(existingCategory);

            patchDocument.ApplyTo(existingCategoryDto);

            var newCategory = _mapper.Map<Category>(existingCategoryDto);
            newCategory.UserId = _userContext.UserId;

            await _repository.UpdateCategory(newCategory);
        }
    }
}