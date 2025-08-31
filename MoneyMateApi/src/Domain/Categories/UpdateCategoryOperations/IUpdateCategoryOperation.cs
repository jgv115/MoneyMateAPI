using System.Threading.Tasks;

namespace MoneyMateApi.Domain.Categories.UpdateCategoryOperations
{
    public interface IUpdateCategoryOperation
    {
        public Task ExecuteOperation();
    }
}