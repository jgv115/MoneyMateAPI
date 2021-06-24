using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Models;
using TransactionService.Repositories;

namespace TransactionService.Domain
{
    public class CategoriesService : ICategoriesService
    {
        private readonly CurrentUserContext _userContext;
        private readonly ICategoriesRepository _repository;

        public CategoriesService(CurrentUserContext userContext, ICategoriesRepository repository)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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

        public Task<IEnumerable<Category>> GetAllCategories()
        {
            return _repository.GetAllCategories(_userContext.UserId);
        }
    }
}