using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories
{
    public class DynamoDbCategoriesRepository : ICategoriesRepository
    {
        private readonly DynamoDBContext _dbContext;
        private readonly string _userId;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Categories";

        public DynamoDbCategoriesRepository(IAmazonDynamoDB dbClient, CurrentUserContext userContext)
        {
            if (dbClient == null)
            {
                throw new ArgumentNullException(nameof(dbClient));
            }

            _userId = userContext.UserId;
            _dbContext = new DynamoDBContext(dbClient);
            _tableName = $"MoneyMate_TransactionDB_{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}";
        }

        private string ExtractPublicFacingUserId(string input) => input.Split("#")[0];

        public async Task<Category> GetCategory(string categoryName)
        {
            var loadedCategory = await _dbContext.LoadAsync<Category>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            if (loadedCategory is not null)
                loadedCategory.UserId = ExtractPublicFacingUserId(loadedCategory.UserId);

            return loadedCategory;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{_userId}{HashKeySuffix}",
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }).GetRemainingAsync();

            userCategoryList.AsParallel().ForAll(category =>
                category.UserId = ExtractPublicFacingUserId(category.UserId));

            return userCategoryList;
        }

        // TODO: convert to index? Check if this works
        public async Task<IEnumerable<Category>> GetAllCategoriesForTransactionType(TransactionType categoryType)
        {
            var userCategoryList = await _dbContext.QueryAsync<Category>($"{_userId}{HashKeySuffix}",
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

        private async Task SaveCategory(Category newCategory) => await _dbContext.SaveAsync(newCategory,
            new DynamoDBOperationConfig
            {
                OverrideTableName = _tableName
            });


        public async Task CreateCategory(Category newCategory)
        {
            var foundCategory = await GetCategory(newCategory.CategoryName);
            if (foundCategory is not null)
            {
                throw new RepositoryItemExistsException(
                    $"Category with name {newCategory.CategoryName} already exists");
            }

            newCategory.UserId += HashKeySuffix;
            await SaveCategory(newCategory);
        }

        public async Task UpdateCategoryName(string categoryName, string newCategoryName)
        {
            // TODO: query transactions that have the existing category name
            // TODO: edit them all to have the new category name
            // TODO: get the category and change the name and push back to DB
        }

        public async Task DeleteCategory(string categoryName)
        {
            // TODO: query transactions to see if there are any with category attached
            // TODO: throw an error if there are
            // TODO: if not, then delete the category
            await _dbContext.DeleteAsync<Category>($"{_userId}{HashKeySuffix}",
                categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }

        public async Task<IEnumerable<string>> GetAllSubcategories(string category)
        {
            var returnedCategory = await _dbContext.LoadAsync<Category>($"{_userId}{HashKeySuffix}", category,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            return returnedCategory.Subcategories;
        }

        public async Task AddSubcategory(string categoryName, string newSubcategory)
        {
            // TODO: query existing category
            // TODO: add new subcategory to it (transaction!)

            var loadedCategory = await _dbContext.LoadAsync<Category>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            loadedCategory.Subcategories.Add(newSubcategory);

            await SaveCategory(loadedCategory);
        }

        public async Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName)
        {
            // TODO: look up transactions that have the subcategory attached
            // TODO: edit them all to have the new subcategory
            // TODO: then edit the category to change the name of the subcategory
        }

        public async Task DeleteSubcategory(string categoryName, string subcategory)
        {
            var category = await _dbContext.LoadAsync<Category>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            category.Subcategories.Remove(subcategory);


            await SaveCategory(category);
        }

        // TODO: deprecate this method
        public Task UpdateCategory(Category updatedCategory)
        {
            return SaveCategory(updatedCategory);
        }
    }
}