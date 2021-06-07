using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransactionService.Domain
{
    public interface ICategoriesService
    {
        public Task<IEnumerable<string>> GetAllCategories();
        public Task<IEnumerable<string>> GetSubCategories(string category);
    }
}