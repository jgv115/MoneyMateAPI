using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Categories;

namespace MoneyMateApi.Repositories
{
    public interface ICategoriesRepository
    {
        public Task<Category> GetCategory(string categoryName);
        public Task<IEnumerable<Category>> GetAllCategories();

        public Task<IEnumerable<Category>> GetAllCategoriesForTransactionType(TransactionType transactionType);

        public Task CreateCategory(Category newCategory);
        public Task UpdateCategoryName(Category category, string newCategoryName);
        public Task DeleteCategory( string categoryName);
        public Task<IEnumerable<string>> GetAllSubcategories(string category);
        public Task AddSubcategory(string categoryName, string newSubcategory);
        public Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName);
        public Task DeleteSubcategory(string categoryName, string subcategory);
    }
}