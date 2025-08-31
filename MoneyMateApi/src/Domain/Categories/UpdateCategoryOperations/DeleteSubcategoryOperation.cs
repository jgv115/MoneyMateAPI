using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Categories.Exceptions;
using MoneyMateApi.Domain.Transactions;
using MoneyMateApi.Repositories;

namespace MoneyMateApi.Domain.Categories.UpdateCategoryOperations
{
    public class DeleteSubcategoryOperation : IUpdateCategoryOperation
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ITransactionHelperService _transactionHelperService;
        private readonly string _existingCategoryName;
        private readonly int _subcategoryIndex;

        public DeleteSubcategoryOperation(ICategoriesRepository categoriesRepository,
            ITransactionHelperService transactionHelperService, string existingCategoryName, int subcategoryIndex)
        {
            _categoriesRepository = categoriesRepository;
            _transactionHelperService = transactionHelperService;
            _existingCategoryName = existingCategoryName;
            _subcategoryIndex = subcategoryIndex;
        }

        public async Task ExecuteOperation()
        {
            var category = await _categoriesRepository.GetCategory(_existingCategoryName);

            if (category is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute DeleteSubcategoryOperation, category {_existingCategoryName} does not exist");

            var subcategoryName = category.Subcategories.ElementAtOrDefault(_subcategoryIndex);
            if (string.IsNullOrEmpty(subcategoryName))
                throw new UpdateCategoryOperationException(
                    $"Failed to execute DeleteSubcategoryOperation, subcategory index {_subcategoryIndex} for category {_existingCategoryName} does not exist");

            var transactions = await _transactionHelperService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Categories = new List<string> {_existingCategoryName},
                Subcategories = new List<string> {subcategoryName}
            });

            if (transactions.Any())
                throw new UpdateCategoryOperationException(
                    $"Failed to execute DeleteSubcategoryOperation, transactions in subcategory {subcategoryName} still exist");

            await _categoriesRepository.DeleteSubcategory(_existingCategoryName, subcategoryName);
        }
    }
}