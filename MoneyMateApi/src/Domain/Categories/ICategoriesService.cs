using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Categories.Dtos;

namespace MoneyMateApi.Domain.Categories
{
    public interface ICategoriesService
    {
        public Task<IEnumerable<string>> GetAllCategoryNames();
        public Task<IEnumerable<string>> GetSubcategories(string category);

        public Task<IEnumerable<Category>> GetAllCategories(TransactionType? transactionType);
        public Task CreateCategory(CategoryDto categoryDto);
        public Task DeleteCategory(string categoryName);
        public Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument);
    }
}