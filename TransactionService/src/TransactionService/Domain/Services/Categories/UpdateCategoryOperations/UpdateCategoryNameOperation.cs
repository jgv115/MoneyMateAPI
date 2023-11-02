using System.Threading.Tasks;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public class UpdateCategoryNameOperation : IUpdateCategoryOperation
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ITransactionHelperService _transactionHelperService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly string _existingCategoryName;
        private readonly string _newCategoryName;

        public UpdateCategoryNameOperation(ICategoriesRepository categoriesRepository,
            ITransactionHelperService transactionHelperService, ITransactionRepository transactionRepository,
            string existingCategoryName,
            string newCategoryName)
        {
            _categoriesRepository = categoriesRepository;
            _transactionHelperService = transactionHelperService;
            _existingCategoryName = existingCategoryName;
            _newCategoryName = newCategoryName;
            _transactionRepository = transactionRepository;
        }


        public async Task ExecuteOperation()
        {
            var existingCategory = await _categoriesRepository.GetCategory(_existingCategoryName);

            if (existingCategory is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute UpdateCategoryNameOperation, category {_existingCategoryName} does not exist");

            await _categoriesRepository.UpdateCategoryName(existingCategory, _newCategoryName);
        }
    }
}