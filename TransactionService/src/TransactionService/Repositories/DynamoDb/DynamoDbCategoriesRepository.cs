using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using TransactionService.Constants;
using TransactionService.Middleware;
using TransactionService.Repositories.DynamoDb.Models;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.DynamoDb
{
    public class DynamoDbCategoriesRepository : ICategoriesRepository
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly string _userId;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Categories";

        public DynamoDbCategoriesRepository(DynamoDbRepositoryConfig config, IDynamoDBContext dbContext, CurrentUserContext userContext)
        {
            _userId = userContext.UserId;
            _dbContext = dbContext;
            _tableName = config.TableName;
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
                OverrideTableName = _tableName,
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

        public async Task UpdateCategoryName(Category category, string newCategoryName)
        {
            await DeleteCategory(category.CategoryName);
            
            category.CategoryName = newCategoryName;
            await CreateCategory(category);
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
            var loadedCategory = await _dbContext.LoadAsync<Category>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            loadedCategory.Subcategories.Remove(subcategoryName);
            loadedCategory.Subcategories.Add(newSubcategoryName);

            await SaveCategory(loadedCategory);
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
    }
}