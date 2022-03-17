using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories
{
    public class DynamoDbCategoriesRepository : ICategoriesRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Categories";

        public DynamoDbCategoriesRepository(IAmazonDynamoDB dbClient)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        private async Task<Category> GetCategory(string userId, string categoryName)
        {
            return await _dbContext.LoadAsync<Category>($"{userId}{HashKeySuffix}", categoryName,
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
            return userCategoryList;
        }

        // TODO: convert to index? Check if this works
        public async Task<IEnumerable<Category>> GetAllCategoriesForTransactionType(string userId,
            TransactionType categoryType)
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{userId}{HashKeySuffix}",
                new DynamoDBOperationConfig
                {
                    QueryFilter = new List<ScanCondition>
                    {
                        new("TransactionType", ScanOperator.Equal, new object[] {categoryType})
                    },
                    OverrideTableName = _tableName
                }).GetRemainingAsync();
            return userCategoryList;
        }

        public async Task<IEnumerable<string>> GetAllSubcategories(string userId, string category)
        {
            var returnedCategory = await _dbContext.LoadAsync<Category>($"{userId}{HashKeySuffix}", category,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            return returnedCategory.Subcategories;
        }

        private async Task SaveCategory(Category newCategory)
        {
            newCategory.UserId += HashKeySuffix;
            await _dbContext.SaveAsync(newCategory, new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });
        }

        public async Task CreateCategory(Category newCategory)
        {
            var foundCategory = await GetCategory(newCategory.UserId, newCategory.CategoryName);
            if (foundCategory is not null)
            {
                throw new RepositoryItemExistsException($"Category with name {newCategory.CategoryName} already exists");
            }

            await SaveCategory(newCategory);
        }
    }
}