using System;
using System.Collections.Generic;
using System.Linq;
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

        private string ExtractPublicFacingUserId(string input) => input.Split("#")[0];

        public async Task<Category> GetCategory(string userId, string categoryName)
        {
            var loadedCategory = await _dbContext.LoadAsync<Category>($"{userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            if (loadedCategory is not null)
                loadedCategory.UserId = ExtractPublicFacingUserId(loadedCategory.UserId);

            return loadedCategory;
        }

        public async Task<IEnumerable<Category>> GetAllCategories(string userId)
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{userId}{HashKeySuffix}",
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }).GetRemainingAsync();

            userCategoryList.AsParallel().ForAll(category =>
                category.UserId = ExtractPublicFacingUserId(category.UserId));

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

            userCategoryList.AsParallel().ForAll(category =>
                category.UserId = ExtractPublicFacingUserId(category.UserId));

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
                throw new RepositoryItemExistsException(
                    $"Category with name {newCategory.CategoryName} already exists");
            }

            await SaveCategory(newCategory);
        }

        public async Task DeleteCategory(string userId, string categoryName)
        {
            await _dbContext.DeleteAsync<Category>($"{userId}{HashKeySuffix}",
                categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }

        public Task UpdateCategory(Category updatedCategory)
        {
            return SaveCategory(updatedCategory);
        }
    }
}