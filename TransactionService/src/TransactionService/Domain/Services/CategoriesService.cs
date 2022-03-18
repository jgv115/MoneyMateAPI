using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.Middleware;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services
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

        public Task<IEnumerable<Category>> GetAllCategories(TransactionType? transactionType = null)
        {
            if (transactionType.HasValue)
                return _repository.GetAllCategoriesForTransactionType(_userContext.UserId, transactionType.Value);
            return _repository.GetAllCategories(_userContext.UserId);
        }

        public Task CreateCategory(CategoryDto categoryDto)
        {
            var newCategory = _mapper.Map<Category>(categoryDto);

            newCategory.UserId = _userContext.UserId;
            return _repository.CreateCategory(newCategory);
        }

        // TODO: error out if no existing category found
        // TODO: enforce one operation at a time.
        // TODO: Can't delete if there are still transactions
        // TODO: if want to change name then have to change it for all transactions
        // TODO: can't change transactionType
        public async Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument)
        {
            var existingCategory = await _repository.GetCategory(_userContext.UserId, categoryName);
            var existingCategoryDto = _mapper.Map<CategoryDto>(existingCategory);

            patchDocument.ApplyTo(existingCategoryDto);

            var newCategory = _mapper.Map<Category>(existingCategoryDto);
            newCategory.UserId = _userContext.UserId;

            await _repository.UpdateCategory(newCategory);
        }
    }
}