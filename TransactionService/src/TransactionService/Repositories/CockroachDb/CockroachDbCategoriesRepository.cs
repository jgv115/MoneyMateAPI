using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;
using TransactionType = TransactionService.Constants.TransactionType;

namespace TransactionService.Repositories.CockroachDb
{
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

        private async Task<IEnumerable<Category>> QueryAndBuildCategory(string query, object parameters = null)
        {
            using (var connection = _context.CreateConnection())
            {
                var categoryDictionary = new Dictionary<Guid, Category>();

                var categories = await connection.QueryAsync<Category, Subcategory, Category>(query,
                    (category, subcategory) =>
                    {
                        Category accumulated_category;

                        if (!categoryDictionary.TryGetValue(category.Id, out accumulated_category))
                        {
                            accumulated_category = category;
                            categoryDictionary.Add(accumulated_category.Id, accumulated_category);
                        }

                        if (subcategory is not null)
                            accumulated_category.Subcategories.Add(subcategory);
                        return accumulated_category;
                    }, parameters);

                return categories;
            }
        }

        public async Task<Domain.Models.Category> GetCategory(string categoryName)
        {
            var query = @"SELECT c.Id, u.Id as UserId, c.name as Name, s.Id, s.Name as Name FROM category c
                            JOIN users u on c.user_id = u.id and u.user_identifier = @user_identifier
                            LEFT JOIN subcategory s on c.id = s.category_id
                            WHERE c.name = @categoryName";

            var categories = await QueryAndBuildCategory(query,
                new {user_identifier = _userContext.UserId, categoryName = categoryName});
            return _mapper.Map<Category, Domain.Models.Category>(categories.Distinct().First());
        }

        public async Task<IEnumerable<Domain.Models.Category>> GetAllCategories()
        {
            var query =
                @"SELECT c.id, u.id as UserId, c.name as Name, tt.name as TransactionType, s.Id, s.name as Name FROM category c
                    JOIN users u ON c.user_id = u.id and u.user_identifier = @user_identifier
                    LEFT JOIN subcategory s on c.id = s.category_id
                    JOIN transactiontype tt on c.transaction_type_id = tt.id";

            var categories = await QueryAndBuildCategory(query,
                new {user_identifier = _userContext.UserId});
            return _mapper.Map<IEnumerable<Category>, List<Domain.Models.Category>>(categories.Distinct());
        }

        public async Task<IEnumerable<Domain.Models.Category>> GetAllCategoriesForTransactionType(
            TransactionType transactionType)
        {
            var query =
                @"SELECT c.id, u.id as UserId, c.name as Name, tt.name as TransactionType, s.Id, s.name as Name FROM category c
                    JOIN users u on c.user_id = u.id
                    LEFT JOIN subcategory s on c.id = s.category_id
                    JOIN transactiontype tt on c.transaction_type_id = tt.id
                    WHERE tt.name = @transaction_type and u.user_identifier = @user_identifier";

            var categories = await QueryAndBuildCategory(query,
                new {user_identifier = _userContext.UserId, transaction_type = transactionType.ToString()});

            return _mapper.Map<IEnumerable<Category>, List<Domain.Models.Category>>(categories.Distinct());
        }

        public async Task CreateCategory(Domain.Models.Category newCategory)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var insertCategoryQuery =
                        @"WITH input (category_name, user_identifier, transaction_type_name) as (VALUES(@new_category_name, @user_identifier, @transaction_type))
                    INSERT INTO category (name, user_id, transaction_type_id)
                    SELECT input.category_name, u.id, tt.id
                    FROM users u
                        JOIN input ON input.user_identifier = u.user_identifier
                        JOIN transactiontype tt ON tt.name = input.transaction_type_name
                    RETURNING id";

                    var categoryParameters = new DynamicParameters();
                    categoryParameters.Add(name: "@new_category_name", value: newCategory.CategoryName, DbType.String);
                    categoryParameters.Add(name: "@user_identifier", value: _userContext.UserId, DbType.String);
                    categoryParameters.Add(name: "@transaction_type", value: newCategory.TransactionType.ToString(),
                        DbType.String);

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
                            newCategory.Subcategories.Select(s => new {subcategory_name = s, category_id = returnedId});
                        await connection.ExecuteAsync(insertSubcategoryQuery, subcategoryParams, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        public Task UpdateCategoryName(Domain.Models.Category category, string newCategoryName)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteCategory(string categoryName)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetAllSubcategories(string category)
        {
            var query =
                @"SELECT subcategory.name FROM subcategory 
                    JOIN category c ON c.id = subcategory.category_id
                    JOIN users u ON u.id = c.user_id and u.user_identifier = @user_identifier
                    WHERE c.name = @category_name";

            using (var connection = _context.CreateConnection())
            {
                var subcategory = await connection.QueryAsync<string>(query,
                    new {user_identifier = _userContext.UserId, category_name = category});

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

        public Task UpdateSubcategoryName(string categoryName, string subcategoryName, string newSubcategoryName)
        {
            throw new System.NotImplementedException();
        }

        public async Task DeleteSubcategory(string categoryName, string subcategory)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                var query = @"
                        DELETE
                        FROM category
                        WHERE category_id = (SELECT id FROM category WHERE category.name = @category_name) 
                        AND name = @subcategory_name";

                await connection.ExecuteAsync(query,
                    new {subcategory_name = subcategory, category_name = categoryName});
            }
        }
    }
}