using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Dtos;
using TransactionService.Models;

namespace TransactionService.Domain
{
    public interface ICategoriesService
    {
        public Task<IEnumerable<string>> GetAllCategoryNames();
        public Task<IEnumerable<string>> GetSubCategories(string category);
        public Task<IEnumerable<Category>> GetAllCategories(string categoryType);
        public Task CreateCategory(CreateCategoryDto createCategoryDto);
    }
}