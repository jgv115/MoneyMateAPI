using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Domain.Models;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories
{
    public class DynamoDbCategoriesRepository : ICategoriesRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Categories";

        private readonly Dictionary<string, string> _rangeKeySuffixes = new()
        {
            {"expense", "expenseCategory#"},
            {"income", "incomeCategory#"}
        };

        public DynamoDbCategoriesRepository(IAmazonDynamoDB dbClient)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        private async Task<Category> GetCategory(string userId, string categoryName, string categoryType)
        {
            return await _dbContext.LoadAsync<Category>($"{userId}{HashKeySuffix}",
                $"{_rangeKeySuffixes[categoryType]}{categoryName}",
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }

        public async Task<IEnumerable<Category>> GetAllCategories(string userId)
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{userId}{HashKeySuffix}",
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }).GetRemainingAsync();
            userCategoryList.AsParallel().ForAll(category =>
                category.CategoryName = category.CategoryName.Split("#")[1]);
            return userCategoryList;
        }

        private async Task<IEnumerable<Category>> GetAllCategoriesForCategoryType(string userId, string categoryType)
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{userId}{HashKeySuffix}",
                QueryOperator.BeginsWith, new[] {_rangeKeySuffixes[categoryType]},
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }).GetRemainingAsync();
            userCategoryList.AsParallel().ForAll(category =>
                category.CategoryName = category.CategoryName.Split("#")[1]);
            return userCategoryList;
        }

        public Task<IEnumerable<Category>> GetAllExpenseCategories(string userId)
        {
            return GetAllCategoriesForCategoryType(userId, "expense");
        }

        public Task<IEnumerable<Category>> GetAllIncomeCategories(string userId)
        {
            return GetAllCategoriesForCategoryType(userId, "income");
        }

        public async Task<IEnumerable<string>> GetAllSubCategories(string userId, string category)
        {
            var returnedCategory = await _dbContext.LoadAsync<Category>($"{userId}{HashKeySuffix}", category,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            return returnedCategory.SubCategories;
        }

        public async Task CreateCategory(Category newCategory, string categoryType)
        {
            var foundCategory = await GetCategory(newCategory.UserId, newCategory.CategoryName, categoryType);
            if (foundCategory is not null)
            {
                throw new RepositoryItemExistsException($"Category of type ${categoryType} already exists");
            }

            newCategory.UserId += HashKeySuffix;
            newCategory.CategoryName = $"{_rangeKeySuffixes[categoryType]}{newCategory.CategoryName}";
            await _dbContext.SaveAsync(newCategory, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }
    }
}