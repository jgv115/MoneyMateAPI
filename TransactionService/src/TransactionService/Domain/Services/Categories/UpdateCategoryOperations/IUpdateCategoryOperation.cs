using System.Threading.Tasks;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public interface IUpdateCategoryOperation
    {
        public Task ExecuteOperation();
    }
}