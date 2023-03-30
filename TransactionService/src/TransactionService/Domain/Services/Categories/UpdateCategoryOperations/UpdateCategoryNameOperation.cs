using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Services.Categories.Exceptions;
using TransactionService.Domain.Services.Transactions;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;

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

            var transactions = await _transactionHelperService.GetTransactionsAsync(new GetTransactionsQuery
            {
                Categories = new List<string> {_existingCategoryName},
            });

            if (transactions.Any())
                foreach (var transaction in transactions)
                {
                    transaction.Category = _newCategoryName;
                    await _transactionRepository.PutTransaction(transaction);
                }

            await _categoriesRepository.UpdateCategoryName(existingCategory, _newCategoryName);
        }
    }
}