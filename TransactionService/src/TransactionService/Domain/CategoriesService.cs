using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using TransactionService.Dtos;
using TransactionService.Models;
using TransactionService.Repositories;

namespace TransactionService.Domain
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

        public Task<IEnumerable<string>> GetSubCategories(string category)
        {
            return _repository.GetAllSubCategories(_userContext.UserId, category);
        }

        public Task<IEnumerable<Category>> GetAllCategories(string categoryType = null)
        {
            return categoryType switch
            {
                "expense" => _repository.GetAllExpenseCategories(_userContext.UserId),
                "income" => _repository.GetAllIncomeCategories(_userContext.UserId),
                _ => _repository.GetAllCategories(_userContext.UserId)
            };
        }

        public Task CreateCategory(CreateCategoryDto createCategoryDto)
        {
            var newCategory = _mapper.Map<Category>(createCategoryDto);

            newCategory.UserId = _userContext.UserId;
            return createCategoryDto.CategoryType switch
            {
                "expense" => _repository.CreateExpenseCategory(newCategory),
                "income" => _repository.CreateIncomeCategory(newCategory),
                _ => throw new ArgumentException("Wrong category type")
            };
        }
    }
}