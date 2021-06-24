using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public interface ICategoriesRepository
    {
        public Task<IEnumerable<Category>> GetAllCategories(string userId);
        public Task<IEnumerable<string>> GetAllSubCategories(string userId, string category);
    }
}