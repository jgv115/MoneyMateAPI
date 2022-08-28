using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Dtos;
using TransactionService.Repositories;

namespace TransactionService.Domain.Services.Categories.UpdateCategoryOperations
{
    public class UpdateSubcategoryNameOperation : IUpdateCategoryOperation
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ITransactionHelperService _transactionHelperService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly string _existingCategoryName;
        private readonly int _subcategoryIndex;
        private readonly string _newSubcategoryName;

        public UpdateSubcategoryNameOperation(ICategoriesRepository categoriesRepository,
            ITransactionHelperService transactionHelperService, ITransactionRepository transactionRepository,
            string existingCategoryName,
            int subcategoryIndex,
            string newSubcategoryName)
        {
            _categoriesRepository = categoriesRepository;
            _transactionHelperService = transactionHelperService;
            _existingCategoryName = existingCategoryName;
            _subcategoryIndex = subcategoryIndex;
            _newSubcategoryName = newSubcategoryName;
            _transactionRepository = transactionRepository;
        }


        public async Task ExecuteOperation()
        {
            var existingCategory = await _categoriesRepository.GetCategory(_existingCategoryName);

            if (existingCategory is null)
                throw new UpdateCategoryOperationException(
                    $"Failed to execute UpdateSubcategoryNameOperation, category {_existingCategoryName} does not exist");

            var subcategoryName = existingCategory.Subcategories.ElementAtOrDefault(_subcategoryIndex);

            var transactions = await _transactionHelperService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Categories = new List<string> {_existingCategoryName},
                Subcategories = new List<string> {subcategoryName}
            });

            if (transactions.Any())
                foreach (var transaction in transactions)
                {
                    transaction.Subcategory = _newSubcategoryName;
                    await _transactionRepository.PutTransaction(transaction);
                }

            await _categoriesRepository.UpdateSubcategoryName(_existingCategoryName, subcategoryName,
                _newSubcategoryName);
        }
    }
}