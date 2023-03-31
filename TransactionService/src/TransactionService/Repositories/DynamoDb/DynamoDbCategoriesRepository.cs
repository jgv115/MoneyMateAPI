using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AutoMapper;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Middleware;
using TransactionService.Repositories.DynamoDb.Models;
using TransactionService.Repositories.Exceptions;

namespace TransactionService.Repositories.DynamoDb
{
    public class DynamoDbCategoriesRepository : ICategoriesRepository
    {
        private readonly IDynamoDBContext _dbContext;
        private readonly IMapper _mapper;
        private readonly string _userId;
        private readonly string _tableName;

        private const string HashKeySuffix = "#Categories";

        public DynamoDbCategoriesRepository(DynamoDbRepositoryConfig config, IDynamoDBContext dbContext,
            CurrentUserContext userContext, IMapper mapper)
        {
            _userId = userContext.UserId;
            _dbContext = dbContext;
            _mapper = mapper;
            _tableName = config.TableName;
        }

        private string ExtractPublicFacingUserId(string input) => input.Split("#")[0];

        public async Task<Category> GetCategory(string categoryName)
        {
            var loadedCategory = await _dbContext.LoadAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            if (loadedCategory is not null)
                loadedCategory.UserId = ExtractPublicFacingUserId(loadedCategory.UserId);

            return _mapper.Map<DynamoDbCategory, Category>(loadedCategory);
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            var userCategoryList = await _dbContext.QueryAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}",
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                }).GetRemainingAsync();

            userCategoryList.AsParallel().ForAll(category =>
                category.UserId = ExtractPublicFacingUserId(category.UserId));

            return _mapper.Map<List<DynamoDbCategory>, List<Category>>(userCategoryList);
        }

        // TODO: convert to index? Check if this works
        public async Task<IEnumerable<Category>> GetAllCategoriesForTransactionType(TransactionType categoryType)
        {
            var userCategoryList = await _dbContext.QueryAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}",
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

            return _mapper.Map<List<DynamoDbCategory>, List<Category>>(userCategoryList);
        }

        private async Task SaveCategory(DynamoDbCategory newDynamoDbCategory) => await _dbContext.SaveAsync(
            newDynamoDbCategory,
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

            var newDynamoDbCategory = _mapper.Map<Category, DynamoDbCategory>(newCategory);

            newDynamoDbCategory.UserId = $"{_userId}{HashKeySuffix}";

            await SaveCategory(newDynamoDbCategory);
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
            await _dbContext.DeleteAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}",
                categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
        }

        public async Task<IEnumerable<string>> GetAllSubcategories(string category)
        {
            var returnedCategory = await _dbContext.LoadAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}", category,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            return returnedCategory.Subcategories;
        }

        public async Task AddSubcategory(string categoryName, string newSubcategory)
        {
            var loadedCategory = await _dbContext.LoadAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });

            loadedCategory.Subcategories.Add(newSubcategory);

            await SaveCategory(loadedCategory);
        }

        public async Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName)
        {
            var loadedCategory = await _dbContext.LoadAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}", categoryName,
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
            var category = await _dbContext.LoadAsync<DynamoDbCategory>($"{_userId}{HashKeySuffix}", categoryName,
                new DynamoDBOperationConfig
                {
                    OverrideTableName = _tableName
                });
            category.Subcategories.Remove(subcategory);


            await SaveCategory(category);
        }
    }
}