using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using TransactionService.Constants;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionService.Repositories.Exceptions;
using TransactionType = TransactionService.Constants.TransactionType;

namespace TransactionService.Repositories.CockroachDb
{
    public static class CategoryDapperHelpers
    {
        public static async Task<IEnumerable<Category>> QueryAndBuildCategories(IDbConnection connection, string query,
            object parameters = null)
        {
            var categoryDictionary = new Dictionary<Guid, Category>();

            var categories = await connection.QueryAsync<Category, Subcategory, Category>(query,
                (category, subcategory) =>
                {
                    Category accumulatedCategory;

                    if (!categoryDictionary.TryGetValue(category.Id, out accumulatedCategory))
                    {
                        accumulatedCategory = category;
                        categoryDictionary.Add(accumulatedCategory.Id, accumulatedCategory);
                    }

                    if (subcategory is not null)
                        accumulatedCategory.Subcategories.Add(subcategory);

                    return accumulatedCategory;
                }, parameters);

            return categories.Distinct();
        }
    }

    public class CockroachDbCategoriesRepository : ICategoriesRepository
    {
        private readonly DapperContext _context;
        private readonly IMapper _mapper;
        private readonly CurrentUserContext _userContext;

        public CockroachDbCategoriesRepository(DapperContext context, IMapper mapper, CurrentUserContext userContext)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
        }


        public async Task<Domain.Models.Category> GetCategory(string categoryName)
        {
            var query =
                @"SELECT c.Id, c.name as Name, tt.name as TransactionType, s.Id, s.Name as Name FROM category c
                            LEFT JOIN subcategory s on c.id = s.category_id
                            JOIN transactiontype tt on c.transaction_type_id = tt.id
                            WHERE c.name = @categoryName and c.profile_id = @profile_id
                            ORDER BY s.name ASC";

            using (var connection = _context.CreateConnection())
            {
                var categories = await CategoryDapperHelpers.QueryAndBuildCategories(connection, query,
                    new {profile_id = _userContext.ProfileId, categoryName});
                return _mapper.Map<Category, Domain.Models.Category>(categories.FirstOrDefault((Category) null));
            }
        }

        public async Task<IEnumerable<Domain.Models.Category>> GetAllCategories()
        {
            var query =
                @"SELECT c.id, c.name as Name, tt.name as TransactionType, s.Id, s.name as Name FROM category c
                    LEFT JOIN subcategory s on c.id = s.category_id
                    JOIN transactiontype tt on c.transaction_type_id = tt.id
                    WHERE c.profile_id = @profile_id
                    ORDER BY c.name, s.name ASC";

            using (var connection = _context.CreateConnection())
            {
                var categories = await CategoryDapperHelpers.QueryAndBuildCategories(connection, query,
                    new {profile_id = _userContext.ProfileId});
                return _mapper.Map<IEnumerable<Category>, List<Domain.Models.Category>>(categories);
            }
        }

        public async Task<IEnumerable<Domain.Models.Category>> GetAllCategoriesForTransactionType(
            TransactionType transactionType)
        {
            var query =
                @"SELECT c.id, c.name as Name, tt.name as TransactionType, s.Id, s.name as Name FROM category c
                    LEFT JOIN subcategory s on c.id = s.category_id
                    JOIN transactiontype tt on c.transaction_type_id = tt.id
                    WHERE tt.name = @transaction_type and c.profile_id = @profile_id
                    ORDER BY s.name ASC";

            using (var connection = _context.CreateConnection())
            {
                var categories = await CategoryDapperHelpers.QueryAndBuildCategories(connection, query,
                    new {profile_id = _userContext.ProfileId, transaction_type = transactionType.ToProperString()});

                return _mapper.Map<IEnumerable<Category>, List<Domain.Models.Category>>(categories);
            }
        }

        public async Task CreateCategory(Domain.Models.Category newCategory)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                var getExistingCategoryQuery = @"SELECT COUNT(1) FROM category 
                                                    JOIN users u ON u.id = category.user_id
                                                    WHERE u.user_identifier = @user_identifier AND category.name = @category_name";

                var categoryFound = await connection.QuerySingleAsync<int>(getExistingCategoryQuery,
                    new {user_identifier = _userContext.UserId, category_name = newCategory.CategoryName});

                if (Convert.ToBoolean(categoryFound))
                    throw new RepositoryItemExistsException(
                        $"Category with name {newCategory.CategoryName} already exists");

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCategoryQuery =
                        @"WITH input (category_name, user_identifier, transaction_type_name, profile_id) as (VALUES(@new_category_name, @user_identifier, @transaction_type, @profile_id))
                    INSERT INTO category (name, user_id, transaction_type_id, profile_id)
                    SELECT input.category_name, u.id, tt.id, input.profile_id
                    FROM users u
                        JOIN input ON input.user_identifier = u.user_identifier
                        JOIN transactiontype tt ON tt.name = input.transaction_type_name
                    RETURNING id";

                    var categoryParameters = new DynamicParameters();
                    categoryParameters.Add(name: "@new_category_name", value: newCategory.CategoryName,
                        DbType.String);
                    categoryParameters.Add(name: "@user_identifier", value: _userContext.UserId, DbType.String);
                    categoryParameters.Add(name: "@transaction_type",
                        value: newCategory.TransactionType.ToProperString(), DbType.String);
                    categoryParameters.Add(name: "@profile_id", value: _userContext.ProfileId);

                    var returnedId =
                        await connection.QuerySingleAsync<Guid>(insertCategoryQuery, categoryParameters, transaction);

                    if (newCategory.Subcategories.Any())
                    {
                        var insertSubcategoryQuery =
                            @"WITH input (subcategory_name, category_id) AS (VALUES(@subcategory_name, @category_id))
                                INSERT INTO subcategory (name, category_id)
                                SELECT input.subcategory_name, input.category_id
                                FROM input";

                        var subcategoryParams =
                            newCategory.Subcategories.Select(s =>
                                new {subcategory_name = s, category_id = returnedId});
                        await connection.ExecuteAsync(insertSubcategoryQuery, subcategoryParams, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        public async Task UpdateCategoryName(Domain.Models.Category category, string newCategoryName)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = @"
                    UPDATE category SET name = @new_category_name
                    WHERE profile_id = @profile_id
                        AND name = @category_name
                    ";

                await connection.ExecuteAsync(query,
                    new
                    {
                        new_category_name = newCategoryName, 
                        profile_id = _userContext.ProfileId,
                        category_name = category.CategoryName
                    });
            }
        }

        public async Task DeleteCategory(string categoryName)
        {
            // TODO: query transactions to see if there are any with category attached
            // TODO: throw an error if there are
            // TODO: if not, then delete the category
            var deleteCategoryQuery = @"DELETE FROM category WHERE profile_id = @profile_id
                                        AND category.name = @category_name";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(deleteCategoryQuery,
                    new {profile_id = _userContext.ProfileId, category_name = categoryName});
            }
        }

        public async Task<IEnumerable<string>> GetAllSubcategories(string category)
        {
            var query =
                @"SELECT subcategory.name FROM subcategory 
                    JOIN category c ON c.id = subcategory.category_id
                    WHERE c.name = @category_name and c.profile_id = @profile_id";

            using (var connection = _context.CreateConnection())
            {
                var subcategory = await connection.QueryAsync<string>(query,
                    new {profile_id = _userContext.ProfileId, category_name = category});

                return subcategory;
            }
        }

        public async Task AddSubcategory(string categoryName, string newSubcategory)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                var query = @"
                        INSERT INTO subcategory(name, category_id)
                        SELECT @subcategory_name, id 
                        FROM category WHERE name = @category_name";

                await connection.ExecuteAsync(query,
                    new {subcategory_name = newSubcategory, category_name = categoryName});
            }
        }

        public async Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName)
        {
            using (var connection = _context.CreateConnection())
            {
                var query = @"
                    UPDATE subcategory SET name = @new_subcategory_name
                    FROM category c
                    WHERE c.profile_id = @profile_id
                        AND c.name = @category_name 
                        AND subcategory.name = @subcategory_name
                    ";

                await connection.ExecuteAsync(query,
                    new
                    {
                        new_subcategory_name = newSubcategoryName, profile_id = _userContext.ProfileId,
                        category_name = categoryName, subcategory_name = subcategoryName
                    });
            }
        }

        public async Task DeleteSubcategory(string categoryName, string subcategory)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                var query = @"
                        DELETE
                        FROM subcategory
                        WHERE category_id = (SELECT id FROM category WHERE category.name = @category_name) 
                        AND name = @subcategory_name";

                await connection.ExecuteAsync(query,
                    new {subcategory_name = subcategory, category_name = categoryName});
            }
        }
    }
}