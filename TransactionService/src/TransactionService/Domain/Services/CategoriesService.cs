using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

        public Task CreateCategory(CreateCategoryDto createCategoryDto)
        {
            var newCategory = _mapper.Map<Category>(createCategoryDto);

            newCategory.UserId = _userContext.UserId;
            return _repository.CreateCategory(newCategory);
        }
    }
}