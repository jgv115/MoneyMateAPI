using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface ICategoriesRepository
    {
        public Task<IEnumerable<Category>> GetAllCategories(string userId);
        public Task<IEnumerable<Category>> GetAllExpenseCategories(string userId);
        public Task<IEnumerable<Category>> GetAllIncomeCategories(string userId);
        public Task<IEnumerable<string>> GetAllSubcategories(string userId, string category);
        public Task CreateCategory(Category newCategory, string catgoryType);
    }
}