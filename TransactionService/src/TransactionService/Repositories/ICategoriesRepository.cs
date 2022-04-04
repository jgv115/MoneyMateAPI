using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface ICategoriesRepository
    {
        public Task<Category> GetCategory(string userId, string categoryName);
        public Task<IEnumerable<Category>> GetAllCategories(string userId);

        public Task<IEnumerable<Category>> GetAllCategoriesForTransactionType(string userId,
            TransactionType transactionType);

        public Task<IEnumerable<string>> GetAllSubcategories(string userId, string category);
        public Task CreateCategory(Category newCategory);
        public Task DeleteCategory(string userId, string categoryName);
        public Task UpdateCategory(Category updatedCategory);
    }
}