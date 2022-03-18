using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Dtos;

namespace TransactionService.Domain.Services
{
    public interface ICategoriesService
    {
        public Task<IEnumerable<string>> GetAllCategoryNames();
        public Task<IEnumerable<string>> GetSubcategories(string category);

        // TODO: don't expose domain model
        public Task<IEnumerable<Category>> GetAllCategories(TransactionType? transactionType);
        public Task CreateCategory(CategoryDto categoryDto);
        public Task UpdateCategory(string categoryName, JsonPatchDocument<CategoryDto> patchDocument);
    }
}