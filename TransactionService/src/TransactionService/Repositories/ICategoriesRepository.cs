using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransactionService.Repositories
{
    public interface ICategoriesRepository
    {
        public Task<IEnumerable<string>> GetAllCategories(string userId);
        public Task<IEnumerable<string>> GetAllSubCategories(string userId, string category);
    }
}