using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;
using TransactionService.Dtos;

namespace TransactionService.Domain.Services
{
    public interface ICategoriesService
    {
        public Task<IEnumerable<string>> GetAllCategoryNames();
        public Task<IEnumerable<string>> GetSubcategories(string category);
        public Task<IEnumerable<Category>> GetAllCategories(string categoryType);
        public Task CreateCategory(CreateCategoryDto createCategoryDto);
    }
}