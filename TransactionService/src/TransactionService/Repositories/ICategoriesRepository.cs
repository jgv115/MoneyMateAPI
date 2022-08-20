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
        public Task CreateCategory(Category newCategory);
        public Task UpdateCategory(Category updatedCategory);
        public Task UpdateCategoryName(string categoryName, string newCategoryName);
        public Task DeleteCategory(string userId, string categoryName);
        public Task<IEnumerable<string>> GetAllSubcategories(string userId, string category);
        public Task AddSubcategory(string userId, string categoryName, string newSubcategory);
        public Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName);
        public Task DeleteSubcategory(string categoryName, string subcategory);
    }
}